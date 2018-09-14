extends "Base.gd"

const Type = 'data'
var Operations = []

func get_data():
	var data_list = []
	var type = null

	for node in self.get_children():
		var data = node.get_data()

		if data.type == Tabby.ERR:
			return data

		if type == null:
			type = data.type

		if not data.type in [Tabby.NUM, Tabby.STR]:
			return Tabby.throw('Unsupported data type "' + Tabby.get_name(data) + '" in math expression', self.line_number)

		if data.type != type:
			return Tabby.throw('All types must be the same in math expression', self.line_number)

		data_list.append(data)

	if len(data_list) > len(Operations)+1:
		return Tabby.throw('To many data inputs in math expression', self.line_number)
	if len(data_list) < len(Operations)+1:
		return Tabby.throw('To many operations in math expression', self.line_number)

	var expression = ''
	var data = null
	for index in len(data_list):
		data = data_list[index]
		if data.type == Tabby.STR:
			expression += "'" + data.data + "'"
		elif data.type == Tabby.NUM:
			expression += 'float(' + str(data.data) + ')'  # Stupid but necessary otherwise division problems are truncated
		else:
			expression += Tabby.to_string(data.data)

		if index+1 != len(data_list):
			expression += Operations[index]

	var out = Tabby.eval_str(expression)
	out = Tabby.malloc(Tabby.get_type(out), out)

	if out.type == Tabby.NUM:
		out.data = Tabby.check_float(out.data)
	return out.dup()
