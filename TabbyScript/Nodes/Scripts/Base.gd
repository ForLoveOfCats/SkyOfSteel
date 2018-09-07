extends Node

var sroot = null
var line_number = null

func execute():
	pass

func get_data():
	pass

func execute_children():
	for child in get_children():
		if sroot.Break:
			break
		child.execute()
