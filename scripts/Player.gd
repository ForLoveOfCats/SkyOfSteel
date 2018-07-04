extends KinematicBody



const MouseSensMultiplyer = 0.2
const MaxAngle = 50
const Gravity = 0.4
const BaseMoveSpeed = 15
const MinimumMoveSpeed = 6
const Friction = BaseMoveSpeed*7
const BaseJumpPush = 10
const ContinueJumpPush = 0.09
const MaxJumpLength = 0.3

const NetTimerLength = 1/60



var MouseSens = 1



var momentum = Vector3(0,0,0)
var movement_multiplyer = 1
var direction = 0
var is_jumping = false
var jump_length = 0

var current_delta = 0

var net_timer = 0
var net_move_time = 0

var possessed = false



func start_jumping():
	#print('start_jumping')
	self.momentum.y = BaseJumpPush
	self.is_jumping = true


func continue_jumping():
	#print('continue_jumping')
	self.momentum.y += ContinueJumpPush


func stop_jumping():
	#print('stop_jumping')
	self.is_jumping = false
	self.jump_length = 0


func set_direction(new_dir):
	self.direction = new_dir
	if self.direction > 360:
		self.direction = self.direction-360
	if self.direction < 0:
		self.direction = self.direction+360
	self.rotation_degrees = Vector3(0,self.direction,0)

remote func update_pos(time, pos, roty):
	if time > self.net_move_time:
		var player = get_tree().get_root().get_node("SteelGame").get_node("SkyScene").get_node(str(get_tree().get_rpc_sender_id()))
		player.rotation_degrees.y = roty
		player.translation = pos
		self.net_move_time = time

remote func send_move_request(time, pos, roty):
	if get_tree().is_network_server():
		if time > self.net_move_time:
			pass
	else:
		rpc_id(1, 'send_move_request', time, pos, roty)


func _ready():
	if self.possessed:
		$SteelCamera.make_current()  # If commented out uses global camera instead of FPS camera
		$FPSMesh.hide()
		add_child(load("res://scenes/SteelHUD.tscn").instance())

func _physics_process(delta):
	if not self.possessed:
		return null

	if self.is_jumping:
		self.jump_length += 1*delta

	if is_on_floor():
		self.momentum.y = clamp(self.momentum.y, -1, 1)

		if Input.is_action_just_pressed("Jump"):
			if not self.is_jumping:
				self.start_jumping()
		else:
			if self.is_jumping:
				self.stop_jumping()
	else:
		if not self.is_jumping:
			self.momentum.y -= Gravity

		if Input.is_action_pressed("Jump") and self.is_jumping and not is_on_ceiling():
			if self.jump_length < self.MaxJumpLength:
				self.continue_jumping()
			else:
				self.stop_jumping()
		else:
			if self.is_jumping:
				self.stop_jumping()

	move_and_slide(self.momentum.rotated(Vector3(0,1,0), deg2rad(self.direction)), Vector3(0,1,0), 0.05, 4, deg2rad(MaxAngle))
	#move_and_slide(self.momentum, Vector3(0,1,0), 0.05, 4, deg2rad(MaxAngle))

func _input(event):
	if not self.possessed:
		return null

	if event is InputEventMouseMotion and SingleSteel.mouse_locked and SingleSteel.player_input_enabled:
		self.set_direction(self.direction+(event.relative[0]*-1*MouseSens*MouseSensMultiplyer))
		$SteelCamera.rotation_degrees.x = clamp(($SteelCamera.rotation_degrees.x + event.relative[1]*-1*MouseSens*MouseSensMultiplyer), -90, 90)

func _process(delta):
	self.current_delta = delta
	if not self.possessed:
		return null

	if Input.is_action_pressed("Sprint"):
		self.movement_multiplyer = 2
	elif Input.is_action_pressed("Crouch"):
		self.movement_multiplyer = 0.4
	else:
		self.movement_multiplyer = 1

	if Input.is_action_just_pressed("TestBind"):
		pass#self.send_move_request(OS.get_ticks_msec(), self.translation, self.rotation_degrees.y)
		#self.translation = Vector3 (0,500,0)
		#OS.shell_open(OS.get_user_data_dir())

	var moving_this_frame_z = false
	if Input.is_action_pressed("MoveForward") and SingleSteel.player_input_enabled:
		self.momentum.z = BaseMoveSpeed*self.movement_multiplyer
		moving_this_frame_z = true
	if Input.is_action_pressed("MoveBack") and SingleSteel.player_input_enabled:
		if not moving_this_frame_z:
			self.momentum.z = BaseMoveSpeed*-1*self.movement_multiplyer
			moving_this_frame_z = true
		else:
			self.momentum.z = 0

	if not moving_this_frame_z and is_on_floor():
		if self.momentum.z > 0:
			self.momentum.z -= Friction*delta
		elif self.momentum.z < 0:
			self.momentum.z += Friction*delta

		if self.momentum.z < MinimumMoveSpeed and self.momentum.z > MinimumMoveSpeed*-1:
			self.momentum.z = 0


	var moving_this_frame_x = false
	if Input.is_action_pressed("MoveLeft") and SingleSteel.player_input_enabled:
		self.momentum.x = BaseMoveSpeed*self.movement_multiplyer
		moving_this_frame_x = true
	if Input.is_action_pressed("MoveRight") and SingleSteel.player_input_enabled:
		if not moving_this_frame_x:
			self.momentum.x = BaseMoveSpeed*-1*self.movement_multiplyer
			moving_this_frame_x = true
		else:
			self.momentum.x = 0

	if not moving_this_frame_x and is_on_floor():
		if self.momentum.x > 0:
			self.momentum.x -= Friction*delta
		elif self.momentum.x < 0:
			self.momentum.x += Friction*delta

		if self.momentum.x < MinimumMoveSpeed and self.momentum.x > MinimumMoveSpeed*-1:
			self.momentum.x = 0

	if self.net_timer == 0:
		rpc_unreliable('update_pos', OS.get_ticks_msec(), self.translation, self.rotation_degrees.y)
	self.net_timer += delta
	if self.net_timer >= NetTimerLength:
		self.net_timer = 0

#OS.get_user_data_dir()
#OS.shell_open(OS.get_user_data_dir())
