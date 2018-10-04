extends Node


const DevMode = false
const DevIp = '127.0.0.1'
const MaxPlayers = 8

var SteelGame = null

var Port = 7777
var mouse_locked = false
var player_input_enabled = true


func spawn_player(id, possess):
	var player = load("res://scenes/Player.tscn").instance()
	player.possessed = possess
	player.set_name(str(id))
	SteelGame.get_node("SkyScene").add_child(player)

func start_world():
	close_world()
	var world = load("res://scenes/SkyScene.tscn").instance()
	world.set_name("SkyScene")
	SteelGame.add_child(world)

	for x in range(0,11):
			for y in range(0,11):
				var platform = load("res://scenes/structures/Platform.tscn").instance()
				platform.translate(Vector3(x*12,0,y*12))
				world.add_child(platform)

func close_world():
	if SteelGame.has_node("SkyScene"):
		SteelGame.get_node("SkyScene").queue_free()

func round_vec(vector):
	return Vector3(round(vector.x), round(vector.y), round(vector.z))

func _ready():
	ScriptEngine.RunNewScript('5+10')
	self.SteelGame = get_tree().get_root().get_node("SteelGame")
