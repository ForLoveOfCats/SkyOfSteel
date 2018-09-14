extends "Base.gd"

const Type = 'exec'
var FuncName = ''
var Variables = {}

func call(args):
	var pointers = []
	for child in get_children():
		var data = child.get_data()
		if data.type == Tabby.ERR:
			return data
		if data.type == Tabby.PTR:
			pointers.append(data)

	if len(args) > len(pointers):
		return Tabby.throw('To many arguments passed to function', self.line_number)

	elif len(args) < len(pointers):
		return Tabby.throw('Not enough arguments passed to function', self.line_number)

	for index in len(pointers):
		self.Variables[pointers[index].data] = args[index]

	execute_children()
	return Tabby.malloc(Tabby.SUC)

func execute():
	self.sroot.Functions[self.FuncName] = self
	return Tabby.malloc(Tabby.SUC)
