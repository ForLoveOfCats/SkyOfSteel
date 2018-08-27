const PermWhite = []
const PermBlack = []

func Call(sroot, args):
	if args[0] != null:
		return 'Expected argument type "null"'

	sroot.cwd.list_dir_begin(true)

	var file_name = sroot.cwd.get_next()
	var found_files = false
	while file_name != "":
		found_files = true
		if sroot.cwd.current_is_dir():
			sroot.call_api('print', ['Folder: ' + file_name])
		else:
			sroot.call_api('print', ['File: ' + file_name])
		file_name = sroot.cwd.get_next()

	if not found_files:
		sroot.call_api('print', ['Folder empty'])

	sroot.cwd.list_dir_end()
