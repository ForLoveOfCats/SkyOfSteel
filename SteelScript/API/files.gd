const PermWhite = []
const PermBlack = []

func Call(sroot, args):
	if args[0] != null:
		return 'Expected argument type "null"'

	OS.shell_open(OS.get_user_data_dir())
