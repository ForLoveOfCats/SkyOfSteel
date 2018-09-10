var sroot

func Call(args):
	if args[0].type != Tabby.NULL:
		return 'Expected argument type "null"'

	Console.printf(sroot.cwd.get_current_dir())
