extends "Base.gd"

const Type = 'data'
var Expression = ''

func get_data():
	var data = []

	var data0 = get_child(0).get_data()
	if data0.type == Tabby.ERR:
		return data0

	var data1 = get_child(1).get_data()
	if data1.type == Tabby.ERR:
		return data1

	match Expression:
		'==':
			data = data0.data == data1.data
		'!=':
			data = data0.data != data1.data
		'>=':
			data = data0.data >= data1.data
		'<=':
			data = data0.data <= data1.data
		'>':
			data = data0.data > data1.data
		'<':
			data = data0.data < data1.data

	return Tabby.malloc(Tabby.get_type(data), data)
