extends "Base.gd"

const Type = 'exec'
var Call = ''

func execute():
	var args = []
	for child in get_children():
		args.append(child.get_data())

	var returned = sroot.call_api(Call, args)

	if typeof(returned) == TYPE_STRING:
		sroot.RuntimeError(returned, self.line_number)

	elif returned == false:
		sroot.RuntimeError('Call to nonexistant function "' + str(Call) + '"', self.line_number)
