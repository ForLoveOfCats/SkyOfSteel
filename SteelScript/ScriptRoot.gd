extends Node

const InvalidNames = ['true', 'false']

var GlobalVars = {}
var Functions = {}
var Break = false

var current_parse_line = 0
var successful_parse = true
var cwd = null
var mode = 'normal'


func load_node(node):
	node = load(self.get_script().get_path().get_base_dir()+"/Nodes/Scenes/"+node+".tscn").instance()
	node.sroot = self
	node.line_number = current_parse_line
	return node


func call_api(call, args):
	call = load(self.get_script().get_path().get_base_dir()+"/API/"+call+".gd")

	if call == null:
		return false

	call = call.new()
	return call.Call(self, args)


func RuntimeError(message, line):
	if not self.Break:
		self.Break = true

		if self.mode == 'console':
			SConsole.printf('RuntimeError: ' + message)
		else:
			SConsole.printf('RuntimeError: ' + message + ' @ line ' + str(line+1))

		if self.mode == 'normal':
			self.queue_free()
		elif self.mode == 'autoexec':
			SConsole.printf('AUTOEXEC FAILED: Not all parts of the autoexec executed successfully. It is highly recommended that you fix your autoexec and restart the game.')
			for child in self.get_children():
				child.queue_free()


func ParseError(message):
	SConsole.printf('ParseError: ' + message + ' @ line ' + str(self.current_parse_line+1))
	self.successful_parse = false

	if self.mode == 'normal':
		self.queue_free()
	elif self.mode == 'autoexec':
		SConsole.printf('AUTOEXEC FAILED: Your autoexec was not executed. It is highly recommended that you fix your autoexec and restart the game.')
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


func remove_open_curly(string):
	var out = ''
	for car in string:
		if car == '{':
			continue
		out += car
	return out


func prepare_paren_str(string):
	var out = ''
	var in_string = false
	for car in string:
		if car == "'":
			in_string = !in_string

		if car == '#' and not in_string:
			break
		out += car
	return out


func list_to_end(string, start):
	var out = []
	for cindex in len(string):
		if cindex >= start:
			out.append(cindex)
	return out


func all_same(string):
	var same = string.substr(0,1)
	for car in string:
		if car != same:
			return false
	return true


func all_plus(string):
	var out = true
	for car in string:
		if car != '+':
			out = false
			break
	return out


func paren_parser(parent, string, index=0):
	string = prepare_paren_str(string)

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
			if opencount > 1:
				childlist.append(paren_parser(null, string, cindex))
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
		node.Data = null

	elif fullstr.is_valid_float():  # It is a number
		node = load_node('Literal')
		node.Data = SteelScript.check_float(float(fullstr))

	elif fullstr[0] in ['+', '-', '*', '/']:  # Math node
		node = load_node('Math')

		for car in fullstr:
			if not car in ['+', '-', '*', '/']:
				ParseError('Invalid character "' + car + '" in math expression')
			node.Operations.append(car)

	elif fullstr[0] in ['=', '>', '<', "!"] and fullstr[1] in ['=', '>', '<', "!"] and len(fullstr) == 2:  # Comparison
		node = load_node('Comparison')
		node.Expression = fullstr

	elif fullstr[0]  == "'":  # String
		if fullstr[len(fullstr)-1] == "'":
			node = load_node('Literal')
			node.Data = fullstr.substr(1,len(fullstr)-2)
		else:
			ParseError('Expected end of string')
			return null

	elif fullstr in ['true', 'false']:  # Boolean
		node = load_node('Literal')
		node.Data = fullstr == 'true'

	else:  # Must be a getvar
		node = load_node('GetVar')
		node.Variable = fullstr

	for child in childlist:
		node.add_child(child)

	if parent != null:
		parent.add_child(node)
		if len(list_to_end(string, index))-endex > 1 and index == 0:
			if string.substr(endex+1, 1) == '(':
				paren_parser(parent, string, endex+1)
	else:
		return node


func parse_line(line, parent):
	if line == '\n':
		return parent

	elif line.substr(0,3) == 'var':
		var SetVar = load_node('SetVar')

		var equaldex = null
		for cindex in len(line)-3:
			if line.substr(3,len(line)-3)[cindex] == '=':
				equaldex = cindex+3
				break
			SetVar.Variable += line.substr(3,len(line)-3)[cindex]

		if SetVar.Variable == '':
			ParseError('Variable name required in variable declaration')
			return parent

		if SetVar.Variable in InvalidNames:
			ParseError('Invalid variable name "' + SetVar.Variable + '" in variable declaration')
			return parent

		if equaldex == null:
			ParseError('Missing equal sign in variable declaration')
			return parent

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

	elif line.substr(0,1) == '}':
		if parent == self:
			ParseError('Cannot close nonexistant block')
			return parent

		parent = parent.get_parent()
		if len(line) > 1:
			if line.substr(1,1) != '#':
				ParseError('Invalid characters following }')
				return parent

	else:  # Must be a Call
		if line.substr(0,1) != '#' and line != '':
			var Call = load_node('Call')

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


func exec_script(script, die):
	if self.cwd == null:
		self.cwd = Directory.new()
		self.cwd.open('user://')

	self.current_parse_line = 0
	self.successful_parse = true

	var parent = self
	var split = strip_white(script).split('\n')

	for lindex in len(split):
		if not self.successful_parse:
			break

		var line = split[lindex]
		self.current_parse_line = lindex

		parent = parse_line(line, parent)

	if parent != self and self.successful_parse:
		ParseError('Block left unclosed from line ' + str(parent.line_number+1))

	if self.successful_parse:
		self.start()

	if die:
		self.queue_free()


func exec_line(line):
	if self.cwd == null:
		self.cwd = Directory.new()
		self.cwd.open('user://')

	self.Break = false
	self.current_parse_line = 0
	self.successful_parse = true

	line = strip_white(line)
	parse_line(line, self)

	var node = self.get_child(len(self.get_children())-1)
	if self.successful_parse:
		node.execute()
	node.queue_free()


func start():
	for child in get_children():
		if Break:
			break
		child.execute()
