extends "Base.gd"

const Type = 'exec'
var Variable = ''

func execute():
	self.sroot.GlobalVars[Variable] = get_children()[0].get_data()
