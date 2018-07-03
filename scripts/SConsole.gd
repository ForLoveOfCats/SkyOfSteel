extends Node



var console_window = null
var command_list = []



func SC_test_command(arg):
	self.printf(str(get_tree().get_network_unique_id()))

func SC_host(arg):
	self.printf('Attempting to host game on port ' + str(SingleSteel.Port))
	SNet.host(SingleSteel.Port)

	if SingleSteel.DevMode:
		OS.execute('godot', ['-dev_connect'], false)

func SC_connect(arg):
	if SingleSteel.DevMode:
		arg = '127.0.0.1'
	if arg == 'localhost':
		arg = '127.0.0.1'
	self.printf('Attempting to connect to ' + str(arg) + ' on port `' + str(SingleSteel.Port))
	SNet.connect(arg, SingleSteel.Port)


func execute_command(command_string):
	self.printf('\n >>> ' + command_string)

	var found_command = false
	var succedded = false
	var needs_arg = true
	var argument = null
	var command_split = command_string.split(' ')

	for current_entry in self.command_list:
		if current_entry[0] == command_split[0]:
			found_command = true
			if not len(command_split) < 2:
				argument = command_string.split(' ')[1]
			needs_arg = current_entry[2]
			succedded = current_entry[1].call_func(argument)

	if not found_command:
		self.printf('Error: Command "' + command_split[0] + '" not found')

	elif not succedded and needs_arg:
		self.printf('Error: Invalid argument "' + str(argument) + '" for command "' + command_split[0] + '"')

func printf(string):
	self.console_window.console_add_line(string)

func logf(string):
	self.console_window.log_add_line(string)



func _ready():
	self.console_window = get_tree().get_root().get_node("SteelGame/ConsoleWindow")
	command_list.append(['test_command', funcref(self, 'SC_test_command'), false])
	command_list.append(['host', funcref(self, 'SC_host'), false])

	if SingleSteel.DevMode:
		command_list.append(['connect', funcref(self, 'SC_connect'), false])
	else:
		command_list.append(['connect', funcref(self, 'SC_connect'), true])
