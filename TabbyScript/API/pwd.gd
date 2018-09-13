var sroot

func Call(args, line):
	if args[0].type != Tabby.NULL:
		return Tabby.throw('Expected argument type "null"', line)

	Console.printf(sroot.cwd.get_current_dir())
	return Tabby.malloc(Tabby.SUC)
