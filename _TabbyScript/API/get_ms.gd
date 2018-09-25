var sroot

func Call(args, line):
	if args[0].type != Tabby.NULL:
		return Tabby.throw('Expected argument type "null"', line)

	return Tabby.malloc(Tabby.NUM, OS.get_ticks_msec())
