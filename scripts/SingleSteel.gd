extends Node 


const DevMode = false
const DevIp = '127.0.0.1'
const MaxPlayers = 8

var SteelGame = null

var Port = 7777
var mouse_locked = false
var player_input_enabled = true


func spawn_player(id, possess):
	SteelGame.spawn_player(id, possess)

func start_world():
	SteelGame.start_world()


func _ready():
	self.SteelGame = get_tree().get_root().get_node("SteelGame")
