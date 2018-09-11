extends "Base.gd"

const Type = 'exec'
var Variable = ''

func execute():
	var data = get_children()[0].get_data()

	if data.type == Tabby.ERR:
		return data

	self.sroot.GlobalVars[Variable] = data
	return Tabby.malloc(Tabby.SUC)
