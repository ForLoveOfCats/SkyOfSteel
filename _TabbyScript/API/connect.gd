var sroot

func Call(args, line):
	if args[0].type != Tabby.STR and args[0].type != Tabby.NULL:
		return Tabby.throw('Expected argument type "string" or "null"', line)

	if args[0].data == 'localhost' or args[0].data == null:
		args[0].data = '127.0.0.1'

	Console.logf('Attempting to connect to "' + args[0].data + '" on port "' + str(Game.Port) + '"')
	Net.connect(args[0].data, Game.Port)
	return Tabby.malloc(Tabby.SUC)
