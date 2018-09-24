extends "Base.gd"

const Type = 'exec'

func execute():
	while true:
		var data = get_child(0).get_data()

		if data.type != Tabby.BOOL:
			return Tabby.throw('Expected "bool" in "while" got "' + Tabby.get_name(data) + '" instead', self.line_number)

		if data.data:
			var returned = execute_children()
			if returned.type == Tabby.ERR:
				return returned
		else:
			break

	return self.success
