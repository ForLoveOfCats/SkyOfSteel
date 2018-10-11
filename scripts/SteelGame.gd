extends Node


func quit():
	get_tree().set_network_peer(null)
	get_tree().quit()


func _notification(what):
	if what == MainLoop.NOTIFICATION_WM_QUIT_REQUEST:
		self.quit()


func _ready():
	get_tree().set_auto_accept_quit(false)
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	Game.MouseLocked = true

	Console.Log('')

	var cmd_args = OS.get_cmdline_args()
	for current_arg in cmd_args:
		Console.Log('Console Argument: ' + current_arg)
		if current_arg == '-dev_connect':
			Console.Execute('connect()')


func _process(delta):
	if Input.is_action_just_pressed("ui_cancel"):
		self.quit()

	if Input.is_action_just_pressed("MouseLock"):
		if Input.get_mouse_mode() == 0:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
			Game.Set("MouseLocked", true)
			Game.Set("PlayerInputEnabled", true)
			$ConsoleWindow.hide()
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
			Game.Set("MouseLocked", false)
			Game.Set("PlayerInputEnabled", false)
			$ConsoleWindow.show()
