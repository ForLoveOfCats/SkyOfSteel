extends "Base.gd"

const Type = 'exec'

func execute():
	while true:
		var data = get_children()[0].get_data()

		if data.type != Tabby.BOOL:
			return Tabby.throw('Expected "bool" in "while" got "' + Tabby.get_name(data) + '" instead', self.line_number)

		if data.data:
			execute_children()
		else:
			break

	return Tabby.malloc(Tabby.SUC)
