extends "Base.gd"

const Type = 'data'
var GetMode = null
var Variable = ''
var ID = null

enum {VAR, FUNC}


func get_data():
	#if Variable[0] == '&':
	#	if not _var_exists(Variable.substr(1,len(Variable)-1)):
	#		self.scope_stack[len(scope_stack)-1].Variables[Variable.substr(1,len(Variable)-1)] = Tabby.malloc(Tabby.NULL)

	#	return Tabby.malloc(Tabby.PTR, Variable.substr(1,len(Variable)-1))

	#elif Variable[0] == '*':
	#	if _var_exists(Variable.substr(1,len(Variable)-1)):
	#		return _get_var(_get_var( Variable.substr(1,len(Variable)-1) ).data)  # Spagetti
	#	else:
	#		return Tabby.throw('No variable or function named "' + Variable.substr(1,len(Variable)-1) + '"', self.line_number)

	if self.GetMode == self.VAR:
		return self.scope.Variables[self.ID]

	else:
		var args = []
		for child in get_children():
			var data = child.get_data()
			if data.type == Tabby.ERR:
				return data
			args.append(data)
		return self.sroot.call_api(Variable, args, self.line_number)

	return Tabby.throw('No variable or function named "' + Variable + '"', self.line_number)
