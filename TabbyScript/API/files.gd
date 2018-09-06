var sroot

func Call(args):
	if args[0] != null:
		return 'Expected argument type "null"'

	OS.shell_open(OS.get_user_data_dir())
