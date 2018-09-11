extends "Base.gd"

const Type = 'exec'
var Call = ''

func execute():
	var args = []
	for child in get_children():
		var data = child.get_data()
		if data.type == Tabby.ERR:
			return data
		args.append(data)

	var returned = sroot.call_api(Call, args)

	if typeof(returned) == TYPE_STRING:
		#sroot.RuntimeError(returned, self.line_number)
		return Tabby.throw(returned, self.line_number)

	elif returned == false:
		#sroot.RuntimeError('Call to nonexistant function "' + str(Call) + '"', self.line_number)
		return Tabby.throw('Call to nonexistant function "' + str(Call) + '"', self.line_number)

	return Tabby.malloc(Tabby.SUC)
