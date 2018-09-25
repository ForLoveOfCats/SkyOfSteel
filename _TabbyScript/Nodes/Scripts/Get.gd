extends "Base.gd"

const Type = 'data'
var GetMode = null
var Variable = ''
var ID = null

enum {VAR, FUNC}


func get_data():
	if self.GetMode == self.VAR:
		return self.scope.Variables[self.ID]

	else:
		var args = []
		for child in get_children():
			var data = child.get_data()
			if data.type == Tabby.ERR:
				return data
			args.append(data)

		return self.sroot.call_func(ID, args, self.line_number)

	return Tabby.throw('No variable or function named "' + Variable + '"', self.line_number)
