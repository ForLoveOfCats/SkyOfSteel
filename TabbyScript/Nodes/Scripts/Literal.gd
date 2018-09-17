extends "Base.gd"

const Type = 'data'
var Data = null

func get_data():
	var data = self.Data

	if data.type == Tabby.NUM:
		data.data = Tabby.check_float(data.data)

	return data
