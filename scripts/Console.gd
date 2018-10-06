extends Node


var console_window = null
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
	Scripting.RunConsoleLine(command_string)


func _ready():
	self.console_window = get_tree().get_root().get_node("SteelGame/ConsoleWindow")
	self.printf('')


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
