extends "Base.gd"

const Type = 'data'
var Variable = ''

func get_data():
	if Variable in self.sroot.GlobalVars:
		return self.sroot.GlobalVars[Variable].dup()

	return Tabby.throw('No variable named "' + Variable + '"', self.line_number)
