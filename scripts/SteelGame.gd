extends Node



func quit():
	get_tree().set_network_peer(null)
	get_tree().quit()

func spawn_player(id, possess):
	var player = load("res://scenes/Player.tscn").instance()
	player.possessed = possess
	player.set_name(str(id))
	get_node("SkyScene").add_child(player)

func start_world():
	if has_node("SkyScene"):
		get_node("SkyScene").queue_free()

	var world = load("res://scenes/SkyScene.tscn").instance()
	world.set_name("SkyScene")
	add_child(world)

func close_world():
	get_node("SkyScene").queue_free()



func _notification(what):
	if what == MainLoop.NOTIFICATION_WM_QUIT_REQUEST:
		self.quit()

func _ready():
	get_tree().set_auto_accept_quit(false)
	Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
	SingleSteel.mouse_locked = true

	SConsole.logf('')
	if SingleSteel.DevMode:
		SConsole.logf('**DEVMODE IS ENABLED**')
		SConsole.logf('')

	var cmd_args = OS.get_cmdline_args()
	for current_arg in cmd_args:
		SConsole.logf('Console Argument: ' + current_arg)
		if current_arg == '-dev_connect':
			SConsole.execute_command('connect localhost')

func _process(delta):
	if Input.is_action_just_pressed("ui_cancel"):
		self.quit()

	if Input.is_action_just_pressed("MouseLock"):
		if Input.get_mouse_mode() == 0:
			Input.set_mouse_mode(Input.MOUSE_MODE_CAPTURED)
			SingleSteel.mouse_locked = true
			SingleSteel.player_input_enabled = true
			$ConsoleWindow.hide()
		else:
			Input.set_mouse_mode(Input.MOUSE_MODE_VISIBLE)
			SingleSteel.mouse_locked = false
			SingleSteel.player_input_enabled = false
			$ConsoleWindow.show()
