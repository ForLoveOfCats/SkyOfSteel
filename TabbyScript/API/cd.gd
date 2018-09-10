var sroot

func Call(args):
	if args[0].type != Tabby.STR:
		return 'Expected argument type "string"'

	if args[0].data in ['', '.', './']:
		return 'Cannot cd to "' + args[0].data + '" no folder specified'

	var returned = sroot.cwd.change_dir(args[0].data)

	if returned == ERR_INVALID_PARAMETER:
		return 'Cannot cd to "' + args[0].data + '" invalid path'
