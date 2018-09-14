extends "Base.gd"

const Type = 'data'
var Variable = ''

func get_data():
	if Variable[0] == '&':
		if not Variable.substr(1,len(Variable)-1) in self.sroot.GlobalVars:
			self.sroot.GlobalVars[Variable.substr(1,len(Variable)-1)] = Tabby.malloc(Tabby.NULL)

		return Tabby.malloc(Tabby.PTR, Variable.substr(1,len(Variable)-1))

	elif Variable[0] == '*':
		return self.sroot.GlobalVars[self.sroot.GlobalVars[Variable.substr(1,len(Variable)-1)].data]  # Spagetti

	elif Variable in self.sroot.GlobalVars:
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
