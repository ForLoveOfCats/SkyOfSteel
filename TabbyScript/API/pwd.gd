var sroot

func Call(args):
	if args[0] != null:
		return 'Expected argument type "null"'

	sroot.call_api('print', [sroot.cwd.get_current_dir()])
