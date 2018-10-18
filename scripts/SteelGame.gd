extends Node


func _notification(what):
	if what == MainLoop.NOTIFICATION_WM_QUIT_REQUEST:
		Game.Quit()



func _process(delta):
	if Input.is_action_just_pressed("ui_cancel"):
		Game.Quit()

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
