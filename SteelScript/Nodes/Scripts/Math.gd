extends "Base.gd"

const Type = 'data'
var Operation = ''

func get_data():
	var data = []
	data.append(get_children()[0].get_data())
	data.append(get_children()[1].get_data())

	for index in [0,1]:
		if typeof(data[index]) == TYPE_STRING:
			print(Operation)
			if not Operation in ['+', '*']:
				sroot.RuntimeError('Invaldid operator "' + Operation + '" for type "string"', self.line_number)
				return null

			data[index] = "'" + SteelScript.to_string(data[index]) + "'"

		elif typeof(data[index]) in [TYPE_INT, TYPE_REAL]:
			data = float(data)

		else:
			sroot.RuntimeError('Invalid data type "' + SteelScript.get_type(data[index]) + '" in math expression "' + Operation + '"', self.line_number)
			return null

		data[index] = SteelScript.to_string(data[index])

	var out = SteelScript.eval_str(data[0]+Operation+data[1])

	if typeof(out) in [TYPE_INT, TYPE_REAL]:
		return SteelScript.check_float(out)
	return out
