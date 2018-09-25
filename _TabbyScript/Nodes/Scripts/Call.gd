extends "Base.gd"

const Type = 'exec'
var Call = ''
var ID = null

func execute():
	var args = []
	for child in get_children():
		var data = child.get_data()
		if data.type == Tabby.ERR:
			return data
		args.append(data)

	return self.sroot.call_func(ID, args, self.line_number)
