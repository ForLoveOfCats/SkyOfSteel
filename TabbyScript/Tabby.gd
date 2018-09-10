extends Node


enum {NULL, NUM, BOOL, STR, ERR}


class DataClass:
	var type = null
	var data = null

	func dup():
		var out = get_script().new()
		out.type = self.type
		out.data = self.data
		return out


func malloc(type, data=null):
	var out = DataClass.new()
	out.type = type
	out.data = data
	return out


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


func get_name(arg):
	var out = 'missing_type'

	match arg.type:
		NULL:
			out = 'null'
		NUM:
			out = 'number'
		BOOL:
			out = 'bool'
		STR:
			out = 'string'
		ERR:
			out = 'error'

	return out


func get_type(arg):
	var out = NULL

	match typeof(arg):
		TYPE_NIL:
			out = NULL
		TYPE_INT:
			out = NUM
		TYPE_REAL:
			out = NUM
		TYPE_BOOL:
			out = BOOL
		TYPE_STRING:
			out = STR

	return out


func exec_script(script):
	var sroot = load(self.get_script().get_path().get_base_dir()+"/ScriptRoot.tscn").instance()
	add_child(sroot)
	sroot.exec_script(script, true)
