extends "Base.gd"

const Type = 'exec'
var Variable = ''
var ID = null


func execute():
	var data = get_child(0).get_data()

	if data.type == Tabby.ERR:
		return data

	self.scope.Variables[ID] = data
	return self.success
