var sroot

func Call(args, line):
	if args[0].type != Tabby.NULL:
		return Tabby.throw('Expected argument type "null"', line)

	OS.shell_open(OS.get_user_data_dir())
	return Tabby.malloc(Tabby.SUC)
