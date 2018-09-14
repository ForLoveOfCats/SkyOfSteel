extends Node


var sroot = null
var scope_stack = []
var line_number = null


func execute():
	return Tabby.malloc(Tabby.SUC)


func get_data():
	return Tabby.malloc(Tabby.SUC)


func execute_children():
	for child in get_children():
		var returned = child.execute()
		if returned.type == Tabby.ERR:
			return returned
