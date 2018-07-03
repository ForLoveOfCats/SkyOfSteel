extends Node


var connect_ip = null
var connect_port = null

var peer_list = []

func host(port):
	self.connect_port = port

	SingleSteel.start_world()

	var peer = NetworkedMultiplayerENet.new()
	peer.create_server(port, SingleSteel.MaxPlayers)
	get_tree().set_network_peer(peer)
	get_tree().set_meta("network_peer", peer)
	SConsole.logf('Started hosting on port ' + str(port))


func connect(ip, port):
	self.connect_ip = ip
	self.connect_port = port

	SingleSteel.start_world()

	var peer = NetworkedMultiplayerENet.new()
	peer.create_client(ip, port)
	get_tree().set_network_peer(peer)
	get_tree().set_meta("network_peer", peer)



func _player_connected(id):
	SConsole.logf('Player ' + str(id) + ' connected')
	SingleSteel.spawn_player(id, false)
	self.peer_list.append(id)

func _player_disconnected(id):
	SConsole.logf('Player ' + str(id) + ' disconnected')
	get_tree().get_root().get_node("SteelGame/SkyScene/" + str(id)).queue_free()
	var index = 0
	for current_id in self.peer_list:
		if current_id == id:
			self.peer_list.remove(index)
			break
		index += 1

func _connected_ok():
	SConsole.logf('Connected to ' + connect_ip + ' on port ' + str(connect_port))

func _server_disconnected():
	SConsole.logf('Lost connection to server at ' + connect_ip + ' on port ' + str(connect_port))
	get_tree().set_network_peer(null)
	SingleSteel.close_world()

func _connected_fail():
	SConsole.logf('Failed to connect to ' + connect_ip + ' on port ' + str(connect_port))



func _ready():
	get_tree().connect("network_peer_connected", self, "_player_connected")
	get_tree().connect("network_peer_disconnected", self, "_player_disconnected")
	get_tree().connect("connected_to_server", self, "_connected_ok")
	get_tree().connect("connection_failed", self, "_connected_fail")
	get_tree().connect("server_disconnected", self, "_server_disconnected")
