extends "Base.gd"

const Type = 'exec'

func execute():
	var data = get_child(0).get_data()

	if data.type != Tabby.BOOL:
		return Tabby.throw('Expected "bool" in "if" got "' + Tabby.get_name(data) + '" instead', self.line_number)

	if data.data:
		execute_children()

	return self.success
