var sroot

func Call(args):
	if args[0].type != Tabby.NULL:
		return 'Expected argument type "null"'

	sroot.cwd.list_dir_begin(true)

	var file_name = sroot.cwd.get_next()
	var found_files = false
	while file_name != "":
		found_files = true
		if sroot.cwd.current_is_dir():
			Console.printf('Folder: ' + file_name)
		else:
			Console.printf('File: ' + file_name)
		file_name = sroot.cwd.get_next()

	if not found_files:
		Console.printf('Folder empty')

	sroot.cwd.list_dir_end()
