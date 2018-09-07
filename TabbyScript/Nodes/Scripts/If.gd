extends "Base.gd"

const Type = 'exec'

func execute():
	var data = get_children()[0].get_data()

	if typeof(data) != TYPE_BOOL:
		sroot.RuntimeError('Expected "bool" in "if" got "' + TabbyScript.get_type(data) + '" instead', self.line_number)

	if get_children()[0].get_data():
		execute_children()
