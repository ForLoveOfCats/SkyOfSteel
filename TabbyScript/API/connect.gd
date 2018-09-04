const PermWhite = []
const PermBlack = []

func Call(sroot, args):
	if typeof(args[0]) != TYPE_STRING and typeof(args[0]) != TYPE_NIL:
		return 'Expected argument type "string" or "null"'

	if args[0] == 'localhost' or args[0] == null:
		args[0] = '127.0.0.1'

	SConsole.logf('Attempting to connect to "' + args[0] + '" on port "' + str(SingleSteel.Port) + '"')
	SNet.connect(args[0], SingleSteel.Port)
