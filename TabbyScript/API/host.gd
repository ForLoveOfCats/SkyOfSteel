var sroot

func Call(args):
	if args[0] != null:
		return 'Expected argument type "null"'

	Net.host(Game.Port)
