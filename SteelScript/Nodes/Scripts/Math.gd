extends "Base.gd"

const Type = 'data'
var Operations = []

func get_data():
	var data_list = []
	var type = typeof(self.get_child(0).get_data())

	if not type in [TYPE_INT, TYPE_REAL, TYPE_STRING]:
		sroot.RuntimeError('Unsupported data type "' + SteelScript.get_type(self.get_child(0).get_data()) + '" in math expression', self.line_number)
		return null

	for node in self.get_children():
		var data = node.get_data()

		if typeof(data) != type:
			sroot.RuntimeError('All types must be the same in math expression', self.line_number)
			return null

		data_list.append(node.get_data())

	if len(data_list) > len(Operations)+1:
		sroot.RuntimeError('To many data inputs in math expression', self.line_number)
		return null
	if len(data_list) < len(Operations)+1:
		sroot.RuntimeError('To many operations in math expression', self.line_number)
		return null

	var expression = ''
	var data = null
	for index in len(data_list):
		data = data_list[index]
		if typeof(data) == TYPE_STRING:
			expression += "'" + data + "'"
		else:
			expression += SteelScript.to_string(data)

		if index+1 != len(data_list):
			expression += Operations[index]

	var out = SteelScript.eval_str(expression)

	if typeof(out) in [TYPE_INT, TYPE_REAL]:
		return SteelScript.check_float(out)
	return out
