extends Node


var connect_ip = null
var connect_port = null


var peers = {}


func host(port):
	self.connect_port = port

	Game.start_world()

	var peer = NetworkedMultiplayerENet.new()
	peer.create_server(port, Game.MaxPlayers)
	get_tree().set_network_peer(peer)
	get_tree().set_meta("network_peer", peer)
	Console.logf('Started hosting on port "' + str(port) + '"')

	self.peers[get_tree().get_network_unique_id()] = 0


func connect(ip, port):
	self.connect_ip = ip
	self.connect_port = port

	Game.start_world()

	var peer = NetworkedMultiplayerENet.new()
	peer.create_client(ip, port)
	get_tree().set_network_peer(peer)
	get_tree().set_meta("network_peer", peer)

	self.peers[get_tree().get_network_unique_id()] = 0


remote func update_pos_id(id, pos):
	get_parent().get_node("SteelGame/SkyScene/" + str(id)).translation = pos


func send_positions(id, pos):
	for current_peer in peers:
		if current_peer != id and current_peer != 1:
			rpc_unreliable_id(current_peer, 'update_pos_id', id, pos)


remote func request_pos(time, pos):
	if not get_tree().is_network_server():
		rpc_id(1, 'request_pos', time, pos)
	else:  # This code is 100% trusted and is running on the server
		var sender = get_tree().get_rpc_sender_id()
		if sender == 0:
			sender = 1

		if time < self.peers[sender]:
			Console.logf('Discarded position request from player "' + str(sender) + '" (out of date)')
		else:  # Do work
			self.peers[sender] = time
			var player = get_parent().get_node("SteelGame/SkyScene/" + str(sender))

			if sender == 1:
				self.send_positions(sender, player.translation)
				return null

			var old_pos = player.translation
			player.move_and_collide(pos-old_pos)

			if not Math.vec_similar(player.translation, pos):
				Console.logf('Rubberbanding player "' + str(sender) + '" due to a movement discrepancy: ' + str(Game.round_vec(player.translation)) + ' != ' + str(Game.round_vec(pos)))
				self.rubberband_player(sender, Game.round_vec(player.translation))
			else:
				player.translation = pos

			self.send_positions(sender, player.translation)


remote func rubberband_player(id, pos):
	if get_tree().get_network_unique_id() != id:
		rpc_id(id, 'rubberband_player', id, pos)
	else:  # Runs on client
		var player = get_parent().get_node("SteelGame/SkyScene/" + str(id))
		player.translation = pos
		player.momentum = Vector3(0,0,0)


remote func sync_rot(rot):
	if get_tree().get_rpc_sender_id() == 0:  # We know that we are running locally on the calling client
		rpc('sync_rot', rot)
	else:
		 get_parent().get_node("SteelGame/SkyScene/" + str(get_tree().get_rpc_sender_id())).rotation_degrees.y = rot


func _player_connected(id):
	Console.logf('Player "' + str(id) + '" connected')
	Game.spawn_player(id, false)
	self.peers[id] = 0


func _player_disconnected(id):
	Console.logf('Player "' + str(id) + '" disconnected')
	get_tree().get_root().get_node("SteelGame/SkyScene/" + str(id)).queue_free()
	self.peers.erase(id)


func _connected_ok():
	Console.logf('Connected to "' + connect_ip + '" on port ' + str(connect_port))


func _server_disconnected():
	Console.logf('Lost connection to server at "' + connect_ip + '" on port "' + str(connect_port) + '"')
	get_tree().set_network_peer(null)
	Game.close_world()


func _connected_fail():
	Console.logf('Failed to connect to "' + connect_ip + '" on port "' + str(connect_port) + '"')


func _ready():
	get_tree().connect("network_peer_connected", self, "_player_connected")
	get_tree().connect("network_peer_disconnected", self, "_player_disconnected")
	get_tree().connect("connected_to_server", self, "_connected_ok")
	get_tree().connect("connection_failed", self, "_connected_fail")
	get_tree().connect("server_disconnected", self, "_server_disconnected")
