extends "Base.gd"

const Type = 'exec'
var Variable = ''
var ID = null


func execute():
	var data = get_child(0).get_data()

	if data.type == Tabby.ERR:
		return data

	#if Variable in self.sroot.Functions or Variable in self.sroot.APIFunctions:
	#	return Tabby.throw('Cannot name variable same as function', self.line_number)

	self.scope.Variables[ID] = data
	return Tabby.malloc(Tabby.SUC)
