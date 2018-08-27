const PermWhite = []
const PermBlack = []

func Call(sroot, args):
	var to_print = ''

	for arg in args:
		arg = SteelScript.to_string(arg)

		if to_print == '':
			to_print += arg
		else:
			to_print += ' ' + arg

	if not sroot.Break:
		SConsole.printf(to_print)
