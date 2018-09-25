var sroot

func Call(args, line):
	if args[0].type != Tabby.NULL:
		return Tabby.throw('Expected argument type "null"', line)

	Net.host(Game.Port)
	return Tabby.malloc(Tabby.SUC)
