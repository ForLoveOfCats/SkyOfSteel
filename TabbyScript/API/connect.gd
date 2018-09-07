var sroot

func Call(args):
	if typeof(args[0]) != TYPE_STRING and typeof(args[0]) != TYPE_NIL:
		return 'Expected argument type "string" or "null"'

	if args[0] == 'localhost' or args[0] == null:
		args[0] = '127.0.0.1'

	Console.logf('Attempting to connect to "' + args[0] + '" on port "' + str(SingleSteel.Port) + '"')
	Net.connect(args[0], SingleSteel.Port)
