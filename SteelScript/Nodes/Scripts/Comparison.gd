extends "Base.gd"

const Type = 'data'
var Expression = ''

func get_data():
	var data = []
	data.append(get_children()[0].get_data())
	data.append(get_children()[1].get_data())

	for index in [0,1]:
		if typeof(data[index]) == TYPE_STRING:
			data[index] = "'" + SteelScript.to_string(data[index]) + "'"
		else:
			data[index] = SteelScript.to_string(data[index])

	return SteelScript.eval_str(data[0]+Expression+data[1])
