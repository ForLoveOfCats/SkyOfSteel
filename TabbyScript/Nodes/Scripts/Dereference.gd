extends "Base.gd"

const Type = 'data'
var Variable = ''
var ID = null


func get_data():
	var pointer = self.scope.Variables[self.ID]

	if pointer.type != Tabby.PTR:
		print('not pointer')
		return Tabby.throw('Cannot dereference type "' + Tabby.get_name(pointer) + '"', self.line_number)

	var refscope = null
	if pointer.data[0] == -1:
		refscope = self.sroot
	else:
		refscope = self.sroot.Functions[pointer.data[0]]

	return refscope.Variables[pointer.data[1]]

