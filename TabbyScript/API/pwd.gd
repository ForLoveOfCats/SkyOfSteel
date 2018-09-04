const PermWhite = []
const PermBlack = []

func Call(sroot, args):
	if args[0] != null:
		return 'Expected argument type "null"'

	sroot.call_api('print', [sroot.cwd.get_current_dir()])
