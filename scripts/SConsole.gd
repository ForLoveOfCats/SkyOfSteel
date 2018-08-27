extends Node


var console_window = null
var sroot = null
var history = []
var hist_local = 0


func printf(string):
	self.console_window.console_add_line(string)


func logf(string):
	self.console_window.log_add_line(string)


func execute_command(command_string):
	self.printf('\n >>> ' + command_string)
	self.history.append(command_string)
	self.hist_local = len(self.history)
	self.sroot.exec_line(command_string)


func _ready():
	self.console_window = get_tree().get_root().get_node("SteelGame/ConsoleWindow")
	self.printf('')
	self.sroot = load("res://SteelScript/ScriptRoot.gd").new()
	self.sroot.set_name('ConsoleScriptRoot')
	self.sroot.mode = 'autoexec'
	add_child(sroot)

	var autoexec = File.new()
	if autoexec.file_exists('user://autoexec.steelscript'):
		autoexec.open('user://autoexec.steelscript', autoexec.READ)
		printf('Autoexec loaded "autoexec.steelscript"')
		sroot.exec_script(autoexec.get_as_text(), false)
	else:
		printf('Autoexec not found "autoexec.steelscript"')

	self.sroot.mode = 'console'


func _process(delta):
	if Input.is_action_just_pressed("ui_up") and self.hist_local > 0:
		self.hist_local -= 1
		self.console_window.get_node('LineEdit').text = self.history[self.hist_local]

	if Input.is_action_just_pressed("ui_down") and self.hist_local < len(self.history):
		self.hist_local += 1
		if self.hist_local == len(self.history):
			self.console_window.get_node('LineEdit').text = ''
		else:
			self.console_window.get_node('LineEdit').text = self.history[self.hist_local]
