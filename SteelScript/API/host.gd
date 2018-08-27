const PermWhite = []
const PermBlack = []

func Call(sroot, args):
	if args[0] != null:
		return 'Expected argument type "null"'

	SNet.host(SingleSteel.Port)
