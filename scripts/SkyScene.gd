extends Node

var setup = false

func _process(delta):
	if get_name() == "SkyScene" and self.setup == false:
		self.setup = true
		SingleSteel.spawn_player(str(get_tree().get_network_unique_id()), true)
	else:
		set_name("SkyScene")
