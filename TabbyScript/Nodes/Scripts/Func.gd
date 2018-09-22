extends "Base.gd"

const Type = 'exec'
var FuncName = ''

var Variables = []
var IDs = {}


func Call(args, call_line):
	var pointers = []
	for child in get_children():
		var data = child.get_data()
		if data.type == Tabby.ERR:
			return data
		if data.type == Tabby.PTR:
			pointers.append(data)

	if len(args) > len(pointers) and len(pointers) != 0:
		return Tabby.throw('To many arguments passed to function', self.line_number)

	elif len(args) < len(pointers):
		return Tabby.throw('Not enough arguments passed to function', self.line_number)

	if len(pointers) != 0:
		for index in len(pointers):
			self.Variables[pointers[index].data] = args[index]

	execute_children()
	return self.success
