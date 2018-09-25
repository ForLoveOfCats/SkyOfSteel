extends Node


var sroot = null
var scope = null
var line_number = null
var success = null


func _init():
	self.success = Tabby.malloc(Tabby.SUC)

func execute():
	return self.success


func get_data():
	return self.success


func execute_children():
	for child in get_children():
		var returned = child.execute()
		if returned.type == Tabby.ERR:
			return returned
	return self.success
