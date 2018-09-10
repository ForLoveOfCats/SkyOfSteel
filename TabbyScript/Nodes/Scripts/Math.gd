extends "Base.gd"

const Type = 'data'
var Operations = []

func get_data():
	var data_list = []
	var type = self.get_child(0).get_data().type

	if not type in [Tabby.NUM, Tabby.STR]:
		sroot.RuntimeError('Unsupported data type "' + Tabby.get_name(self.get_child(0).get_data()) + '" in math expression', self.line_number)
		return null

	for node in self.get_children():
		var data = node.get_data()

		if data.type != type:
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
		if data.type == Tabby.STR:
			expression += "'" + data.data + "'"
		else:
			expression += Tabby.to_string(data.data)

		if index+1 != len(data_list):
			expression += Operations[index]

	var out = Tabby.eval_str(expression)
	out = Tabby.malloc(Tabby.get_type(out), out)

	if out.type == Tabby.NUM:
		out.data = Tabby.check_float(out.data)
	return out.dup()
