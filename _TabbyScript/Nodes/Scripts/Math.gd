extends "Base.gd"


const Type = 'data'
var Operations = []
var OpCount = null


func _list_to_end(list, index):
	var out = []
	for entrydex in len(list):
		if entrydex >= index:
			out.append(list[entrydex])
	return out


func get_data():
	var expression = []
	var type = null

	var ndex = -1
	for node in self.get_children():
		ndex += 1
		var data = node.get_data()

		if data.type == Tabby.ERR:
			return data

		if type == null:
			type = data.type

		if not data.type in [Tabby.NUM, Tabby.STR]:
			return Tabby.throw('Unsupported data type "' + Tabby.get_name(data) + '" in math expression', self.line_number)

		if data.type != type:
			return Tabby.throw('All types must be the same in math expression', self.line_number)

		expression.append(data.data)
		if ndex <= self.OpCount:
			expression.append(self.Operations[ndex])

	while len(expression) > 1:
		var multi = '*' in expression or '/' in expression
		var new_ex = []
		for entrydex in len(expression):
			var entry = expression[entrydex]

			if entry in ['*', '/']:
				match entry:
					'*':
						new_ex.pop_back()
						new_ex.append( expression[entrydex-1]*expression[entrydex+1] )
						new_ex = new_ex + _list_to_end(expression, entrydex+2)
						break
					'/':
						new_ex.pop_back()
						new_ex.append( float(expression[entrydex-1])/float(expression[entrydex+1]) )
						new_ex = new_ex + _list_to_end(expression, entrydex+2)
						break
			else:
				if multi:
					new_ex.append(entry)
				else:
					match entry:
						'+':
							new_ex.pop_back()
							new_ex.append( expression[entrydex-1]+expression[entrydex+1] )
							new_ex = new_ex + _list_to_end(expression, entrydex+2)
							break
						'-':
							new_ex.pop_back()
							new_ex.append( expression[entrydex-1]-expression[entrydex+1] )
							new_ex = new_ex + _list_to_end(expression, entrydex+2)
							break
		expression = new_ex

	var out = expression[0]
	out = Tabby.malloc(type, out)

	if out.type == Tabby.NUM:
		out.data = Tabby.check_float(out.data)
	return out
