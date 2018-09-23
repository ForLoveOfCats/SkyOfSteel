extends Node

const InvalidNames = ['true', 'false']
const InvalidCars = [':', '/', '.', '&', '*', '{', '}', '[', ']', '(', ')', '!']

var Variables = []
var IDs = {}
var GetNodes = []
var Functions = []
var FuncIDs = {}
var CallNodes = []
var APIFunctions = {}

var current_parse_line = 0
var successful_parse = true
var cwd = null
var mode = 'normal'



func _init():
	var api_dir = Directory.new()
	api_dir.open(self.get_script().get_path().get_base_dir()+'/API')

	api_dir.list_dir_begin(true)
	var file_name = api_dir.get_next()
	while file_name != "":
		var call = load(self.get_script().get_path().get_base_dir()+"/API/"+file_name).new()
		call.sroot = self
		self.FuncIDs[file_name.get_basename()] = len(Functions)
		self.Functions.append(call)
		file_name = api_dir.get_next()


func load_node(node):
	node = load(self.get_script().get_path().get_base_dir()+"/Nodes/Scenes/"+node+".tscn").instance()
	node.sroot = self
	node.line_number = current_parse_line
	return node


func call_func(id, args, line):
	return self.Functions[id].Call(args, line)


func RuntimeError(message, line):
	if self.mode == 'console':
		Console.printf('RuntimeError: ' + message)
	else:
		Console.printf('RuntimeError: ' + message + ' @ line ' + str(line+1))

	if self.mode == 'normal':
		self.queue_free()
	elif self.mode == 'autoexec':
		Console.printf('AUTOEXEC FAILED: Not all parts of the autoexec executed successfully. It is highly recommended that you fix your autoexec and restart the game.')
		for child in self.get_children():
			child.queue_free()


func ParseError(message):
	Console.printf('ParseError: ' + message + ' @ line ' + str(self.current_parse_line+1))
	self.successful_parse = false

	if self.mode == 'normal':
		self.queue_free()
	elif self.mode == 'autoexec':
		Console.printf('AUTOEXEC FAILED: Your autoexec was not executed. It is highly recommended that you fix your autoexec and restart the game.')
		for child in self.get_children():
			child.queue_free()


func strip_white(script):
	var out = ''
	var in_string = false

	for index in len(script):
		if script[index] == "'":
			in_string = !in_string
			out += script[index]
			continue

		if in_string or script[index] != ' ' and script[index] != '	' :
			out += script[index]

	return out


func strip_comments(script):
	var out = ''
	var in_comment = false
	var in_string = false

	for car in script:
		if car == "'":
			in_string = !in_string
		elif car == '#' and not in_string:
			in_comment = true
		elif car == '\n':
			in_comment = false

		if not in_comment:
			out += car

	return out

func remove_open_curly(string):
	var out = ''
	for car in string:
		if car == '{':
			continue
		out += car
	return out


func list_to_end(string, start):
	var out = []
	for cindex in len(string):
		if cindex >= start:
			out.append(cindex)
	return out


func all_in(string, list):
	for car in string:
		if not car in list:
			return false
	return true


func invalid_name(name):
	if name in InvalidNames or name.is_valid_integer():
		return true

	for car in name:
		if car in InvalidCars:
			return true

	return false


func paren_parser(parent, string, index=0, should_return=false):
	var scope = null
	var scope_item = parent
	while true:
		if scope_item is self.get_script():
			scope = scope_item
			break
		elif scope_item is preload("Nodes/Scripts/Func.gd"):
			scope = scope_item
		scope_item = scope_item.get_parent()

	var opencount = 0
	var carlist = []
	var childlist = []

	var endex = 0
	var in_string = false
	for cindex in list_to_end(string, index):
		var car = string[cindex]

		if car == "'":
			in_string = !in_string

		elif car == '(' and not in_string:
			opencount += 1
			if opencount == 2:
				childlist.append(paren_parser(parent, string, cindex, true))
			continue

		elif car == ')' and not in_string:
			opencount -= 1
			if opencount <= 0:
				endex = cindex
				break
			continue

		if opencount <= 1:
			carlist.append([car, opencount])

	if in_string:
		ParseError('Expected string end before line end')
		return null

	if opencount > 0:
		ParseError('Unclosed parentheses')
		return null

	if len(list_to_end(string, index))-endex > 1 and index == 0:
		if string.substr(endex+1, 1) != '(':
			ParseError('Invalid characters following parentheses')
			return null

	var fullstr = ''
	for car in carlist:
		fullstr += car[0]

	# Begin parsing!
	var node = null

	if fullstr == '':
		node = load_node('Literal')
		node.Data = Tabby.malloc(Tabby.NULL)

	elif fullstr.is_valid_float():  # It is a number
		node = load_node('Literal')
		node.Data = Tabby.malloc(Tabby.NUM, Tabby.check_float(float(fullstr)))

	elif all_in(fullstr, ['+', '-', '*', '/']):  # Math node
		if len(childlist) > len(fullstr)+1:
			ParseError('To many data inputs in math expression')
			return null
		if len(childlist) < len(fullstr)+1:
			ParseError('To many operations in math expression')
			return null

		node = load_node('Math')
		for car in fullstr:
			node.Operations.append(car)
		node.OpCount = len(fullstr)-1

	elif fullstr[0] in ['=', '>', '<', "!"]:  # Comparison
		if len(fullstr) > 2:
			ParseError('To many operators in comparison')
			return null

		if len(fullstr) == 2 and fullstr[1] != '=':
			ParseError('Cannot have "' + fullstr[1] + '" following "' + fullstr[0] + '" in comparison')
			return null

		node = load_node('Comparison')
		node.Expression = fullstr

	elif fullstr[0]  == "'":  # String
		if fullstr[len(fullstr)-1] == "'":
			node = load_node('Literal')
			node.Data = Tabby.malloc(Tabby.STR, fullstr.substr(1,len(fullstr)-2))
		else:
			ParseError('Expected end of string')
			return null

	elif fullstr in ['true', 'false']:  # Boolean
		node = load_node('Literal')
		node.Data = Tabby.malloc(Tabby.BOOL, fullstr == 'true')

	else:  # Must be a getvar
		node = load_node('Get')
		node.Variable = fullstr
		node.scope = scope
		self.GetNodes.append(node)

	for child in childlist:
		node.add_child(child)

	if should_return:
		return node
	else:
		parent.add_child(node)
		if len(list_to_end(string, index))-endex > 1 and index == 0:
			if string.substr(endex+1, 1) == '(':
				paren_parser(parent, string, endex+1)


func parse_line(line, parent):
	if line == '\n':
		return parent

	elif line.substr(0,3) == 'var':
		var scope = null
		var scope_item = parent
		while true:
			if scope_item is self.get_script():
				scope = scope_item
				break
			elif scope_item is preload("Nodes/Scripts/Func.gd"):
				scope = scope_item
			scope_item = scope_item.get_parent()

		var SetVar = load_node('SetVar')
		SetVar.scope = scope

		var equaldex = null
		for cindex in len(line)-3:
			if line.substr(3,len(line)-3)[cindex] == '=':
				equaldex = cindex+3
				break
			SetVar.Variable += line.substr(3,len(line)-3)[cindex]

		if equaldex == null:
			ParseError('Missing equal sign in variable declaration')
			return parent

		if SetVar.Variable == '':
			ParseError('Variable name required in variable declaration')
			return parent

		if invalid_name(SetVar.Variable):
			ParseError('Invalid variable name "' + SetVar.Variable + '" in variable declaration')
			return parent

		if SetVar.Variable in SetVar.scope.IDs:
			SetVar.ID = SetVar.scope.IDs[SetVar.Variable]
		else:
			SetVar.ID = len(SetVar.scope.IDs)
			SetVar.scope.IDs[SetVar.Variable] = SetVar.ID
			SetVar.scope.Variables.append(Tabby.malloc(Tabby.NULL))

		parent.add_child(SetVar)
		paren_parser(SetVar, line.substr(equaldex+1,len(line)-equaldex+1))
		return parent

	elif line.substr(0,2) == 'if':
		if self.mode == 'console':
			ParseError('Cannot use if statement in console')
			return parent

		if line.substr(2, 1) != '(':
			ParseError('Missing parentheses in "if"')
			return parent

		var If = load_node('If')
		parent.add_child(If)

		paren_parser(If, remove_open_curly(line.substr(2, len(line)-2)))
		parent = If
		return parent

	elif line.substr(0,5) == 'while':
		if self.mode == 'console':
			ParseError('Cannot use while statement in console')
			return parent

		if line.substr(5, 1) != '(':
			ParseError('Missing parentheses in "while"')
			return parent

		var While = load_node('While')
		parent.add_child(While)

		paren_parser(While, remove_open_curly(line.substr(5, len(line)-5)))
		parent = While
		return parent

	elif line.substr(0,4) == 'func':
		if self.mode == 'console':
			ParseError('Cannot declare function in console')
			return parent

		var parendex = null
		var func_name = ''
		for cindex in len(line)-4:
			if line.substr(4,len(line)-4)[cindex] == '(':
				parendex = cindex+4
				break
			func_name += line.substr(4,len(line)-4)[cindex]

		if parendex == null:
			ParseError('Missing parentheses in function declaration')
			return parent

		var Func = load_node('Func')

		if func_name in self.FuncIDs:
			ParseError('Function "' + func_name +  '" already exists')
			return parent
		else:
			self.FuncIDs[func_name] = len(Functions)
			self.Functions.append(Func)

		Func.FuncName = func_name
		parent.add_child(Func)

		paren_parser(Func, remove_open_curly(line.substr(parendex, len(line)-parendex)))
		parent = Func
		return parent

	elif line.substr(0,1) == '}':
		if parent == self:
			ParseError('Cannot close nonexistant block')
			return parent

		parent = parent.get_parent()

	else:  # Must be a Call
		if line != '':
			var Call = load_node('Call')
			self.CallNodes.append(Call)

			var parendex = null
			for cindex in len(line):
				var car = line[cindex]

				if car == '(':
					parendex = cindex
					break
				else:
					Call.Call += car

			if Call.Call == '':
				ParseError('Missing call name in API call')
				return parent

			parent.add_child(Call)

			var paren = null
			if parendex == null:
				paren_parser(Call, '()')
			else:
				paren_parser(Call, line.substr(parendex,len(line)-parendex))

			return parent

	return parent


func prepare_get_nodes():
	for get in self.GetNodes:
		if get.Variable in get.scope.IDs:
			get.ID = get.scope.IDs[get.Variable]
			get.GetMode = get.VAR

		elif get.Variable in self.FuncIDs:
			get.ID = self.FuncIDs[get.Variable]
			get.GetMode = get.FUNC

		else:
			self.current_parse_line = get.line_number
			ParseError('No variable or function named "' + get.Variable + '"')
			break
	self.GetNodes = []


func prepare_call_nodes():
	for call in self.CallNodes:
		if call.Call in self.FuncIDs:
			call.ID = self.FuncIDs[call.Call]
		else:
			self.current_parse_line = call.line_number
			ParseError('No function named "' + call.Call + '"')
			break
	self.CallNodes = []


func exec_script(script, die):
	if self.cwd == null:
		self.cwd = Directory.new()
		self.cwd.open('user://')

	self.current_parse_line = 0
	self.successful_parse = true

	var parent = self
	var split = strip_comments(strip_white(script)).split('\n')

	for lindex in len(split):
		if not self.successful_parse:
			break

		var line = split[lindex]
		self.current_parse_line = lindex

		parent = parse_line(line, parent)
	prepare_get_nodes()
	prepare_call_nodes()

	if parent != self and self.successful_parse:
		ParseError('Block left unclosed from line ' + str(parent.line_number+1))

	if self.successful_parse:
		self.start()

	if die:
		self.queue_free()


func exec_line(line):
	if line == '':
		return 0

	if self.cwd == null:
		self.cwd = Directory.new()
		self.cwd.open('user://')

	self.current_parse_line = 0
	self.successful_parse = true

	line = strip_comments(strip_white(line))
	parse_line(line, self)
	prepare_get_nodes()
	prepare_call_nodes()

	if self.successful_parse:
		var node = self.get_child(len(self.get_children())-1)
		var returned = node.execute()
		if returned.type == Tabby.ERR:
			RuntimeError(returned.message, returned.line)
		node.queue_free()


func start():
	for child in get_children():
		var returned = child.execute()
		if returned.type == Tabby.ERR:
			RuntimeError(returned.message, returned.line)
			break
