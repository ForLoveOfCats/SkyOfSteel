var sroot

func Call(args, line):
	var converted = ''

	for arg in args:
		arg = Tabby.to_string(arg.data)

		if converted == '':
			converted += arg
		else:
			converted += ' ' + arg

	return Tabby.malloc(Tabby.STR, converted)
