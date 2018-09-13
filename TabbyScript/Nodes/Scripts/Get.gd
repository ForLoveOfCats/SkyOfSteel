extends "Base.gd"

const Type = 'data'
var Variable = ''

func get_data():
	if Variable in self.sroot.GlobalVars:
		return self.sroot.GlobalVars[Variable].dup()
	elif Variable in self.sroot.APIFunctions:
		var args = []
		for child in get_children():
			var data = child.get_data()
			if data.type == Tabby.ERR:
				return data
			args.append(data)
		return self.sroot.call_api(Variable, args, self.line_number)

	return Tabby.throw('No variable or function named "' + Variable + '"', self.line_number)
