const PermWhite = []
const PermBlack = []

func Call(sroot, args):
	if typeof(args[0]) != TYPE_STRING:
		return 'Expected argument type "string"'

	if args[0] in ['', '.', './', '/']:
		return 'Cannot cd to "' + args[0] + '" no folder specified'

	var returned = sroot.cwd.change_dir(args[0])

	if returned == ERR_INVALID_PARAMETER:
		return 'Cannot cd to "' + args[0] + '" invalid path'
