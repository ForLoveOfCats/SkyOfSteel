var sroot

func Call(args):
	if args[0].type != Tabby.NULL:
		return 'Expected argument type "null"'

	Net.host(Game.Port)
