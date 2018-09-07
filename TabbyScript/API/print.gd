var sroot

func Call(args):
	var to_print = ''

	for arg in args:
		arg = Tabby.to_string(arg)

		if to_print == '':
			to_print += arg
		else:
			to_print += ' ' + arg

	if not sroot.Break:
		Console.printf(to_print)
