extends "Base.gd"

const Type = 'data'
var Data = null

func get_data():
	var data = self.Data

	if typeof(data) in [TYPE_INT, TYPE_REAL]:
		data = Tabby.check_float(data)

	return self.Data
