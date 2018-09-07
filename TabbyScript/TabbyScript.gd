extends Node


const InvalidNames = ['true', 'false']


func eval_str(input):
	var script = GDScript.new()
	script.set_source_code('func eval():\n\treturn ' + input)
	script.reload()

	var obj = Reference.new()
	obj.set_script(script)

	return obj.eval()


func check_float(infloat):
	if infloat == int(infloat):
		infloat = int(infloat)
	return infloat


func to_string(arg):
	if typeof(arg) in [TYPE_BOOL, TYPE_NIL]:
		arg = str(arg).to_lower()
	elif typeof(arg) == TYPE_BOOL:
		if arg == true:
			arg = 'true'
		else:
			arg = 'false'
	else:
		arg = str(arg)

	return arg


func get_type(arg):
	var out = 'missing_type'

	match typeof(arg):
		TYPE_NIL:
			out = 'null'
		TYPE_BOOL:
			out = 'bool'
		TYPE_STRING:
			out = 'string'
		TYPE_INT:
			out = 'number'
		TYPE_REAL:
			out = 'number'

	return out


func exec_script(script):
	var sroot = load(self.get_script().get_path().get_base_dir()+"/ScriptRoot.tscn").instance()
	add_child(sroot)
	sroot.exec_script(script, true)
