var sroot

func Call(args, line):
	if args[0].type != Tabby.STR:
		return Tabby.throw('Expected argument type "string"', line)

	if args[0].data in ['', '.', './']:
		return Tabby.throw('Cannot cd to "' + args[0].data + '" no folder specified', line)

	var returned = sroot.cwd.change_dir(args[0].data)

	if returned == ERR_INVALID_PARAMETER:
		return Tabby.throw('Cannot cd to "' + args[0].data + '" invalid path', line)

	return Tabby.malloc(Tabby.SUC)
