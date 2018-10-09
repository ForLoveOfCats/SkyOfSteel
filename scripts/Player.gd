extends KinematicBody


const MouseSensMultiplyer = 0.2
const MaxAngle = 50
const Gravity = 0.4
const BaseMoveSpeed = 18
const AirstrafeBoostMultiplyer = 0.01
const AirstrafeMaxBoost = 65
const MinimumMoveSpeed = 6
const Friction = BaseMoveSpeed*10
const LandSlowMultiplyer = 0.84
const BaseJumpPush = 10
const ContinueJumpPush = 0.09
const MaxJumpLength = 0.3

const NetTimerLength = 1/60

var MouseSens = 1

var momentum = Vector3(0,0,0)
var movement_multiplyer = 1
var direction = 0
var air_direction = 0
var is_jumping = false
var jump_length = 0
var on_floor_last_frame = false

var health = 0
var inventory = [null,null,null,null,null,null,null,null,null,null]
var slot = 0

var net_move_time = 0

var possessed = false


func airstrafe(rot):
	if Input.is_action_pressed("MoveForward") or Input.is_action_pressed("MoveBack"):
		return null

	self.air_direction = self.direction

	if not Input.is_action_pressed("Sprint"):
		return null
	if round(self.momentum.z) >= 0:
		self.momentum.z += abs(rot)*AirstrafeBoostMultiplyer
	elif round(self.momentum.z) < 0:
		self.momentum.z -= abs(rot)*AirstrafeBoostMultiplyer
	self.momentum.z = clamp(self.momentum.z, AirstrafeMaxBoost*-1, AirstrafeMaxBoost)


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


func give_item(item):
	var added = false
	for slot in len(inventory):
		if inventory[slot] != null:  # Actual item in slot
			if inventory[slot].name == item.name:
				inventory[slot].count += item.count
				added = true

	if not added:
		for slot in len(inventory):
			if inventory[slot] == null:
				inventory[slot] = item
				break


func _init():
	give_item(Items.return_instance('Platform'))
	give_item(Items.return_instance('Wall'))
	give_item(Items.return_instance('Slope'))


func _ready():
	self.translation = Vector3(0,1,0)
	if self.possessed:
		$SteelCamera.make_current()  # If commented out uses global camera instead of FPS camera
		$FPSMesh.hide()
		add_child(load("res://scenes/SteelHUD.tscn").instance())


func _physics_process(delta):
	if not self.possessed:
		return null

	if Input.is_action_pressed("Sprint") and Game.PlayerInputEnabled:
		self.movement_multiplyer = 2
	elif Input.is_action_pressed("Crouch") and Game.PlayerInputEnabled:
		self.movement_multiplyer = 0.4
	else:
		self.movement_multiplyer = 1

	if Input.is_action_just_pressed("TestBind") and Game.PlayerInputEnabled:
		self.translation = Vector3(0,0,0)
		#Net.request_pos(OS.get_ticks_msec(), Vector3(0,5,20))
		#OS.shell_open(OS.get_user_data_dir())

	var moving_this_frame_z = false
	if Input.is_action_pressed("MoveForward") and Game.PlayerInputEnabled and is_on_floor() and self.on_floor_last_frame:
		self.momentum.z = BaseMoveSpeed*self.movement_multiplyer
		moving_this_frame_z = true
	if Input.is_action_pressed("MoveBack") and Game.PlayerInputEnabled and is_on_floor() and self.on_floor_last_frame:
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
	if Input.is_action_pressed("MoveLeft") and Game.PlayerInputEnabled and is_on_floor() and self.on_floor_last_frame:
		self.momentum.x = BaseMoveSpeed*self.movement_multiplyer
		moving_this_frame_x = true
	if Input.is_action_pressed("MoveRight") and Game.PlayerInputEnabled and is_on_floor() and self.on_floor_last_frame:
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

	if self.is_jumping:
		self.jump_length += 1*delta


	if is_on_floor():
		self.momentum.y = clamp(self.momentum.y, -1, 1)

		if Input.is_action_pressed("Jump") and Game.PlayerInputEnabled:
			if not self.is_jumping:
				self.start_jumping()
		else:
			if self.is_jumping:
				self.stop_jumping()

		if not self.on_floor_last_frame:
			self.momentum.y *= LandSlowMultiplyer
			self.momentum.z *= LandSlowMultiplyer
	else:
		if not self.is_jumping:
			self.momentum.y -= Gravity

		if Input.is_action_pressed("Jump") and self.is_jumping and not is_on_ceiling() and Game.PlayerInputEnabled:
			if self.jump_length < self.MaxJumpLength:
				self.continue_jumping()
			else:
				self.stop_jumping()
		else:
			if self.is_jumping:
				self.stop_jumping()

	Net.request_pos(OS.get_ticks_msec(), self.translation)
	Net.sync_rot(self.rotation_degrees.y)

	if is_on_floor():
		if self.on_floor_last_frame:
			self.air_direction = self.direction
		self.on_floor_last_frame = true
		move_and_slide(self.momentum.rotated(Vector3(0,1,0), deg2rad(self.direction)), Vector3(0,1,0), 0.05, 4, deg2rad(MaxAngle)).rotated(Vector3(0,1,0), deg2rad(-self.direction))
	else:
		self.on_floor_last_frame = false
		move_and_slide(self.momentum.rotated(Vector3(0,1,0), deg2rad(self.air_direction)), Vector3(0,1,0), 0.05, 4, deg2rad(MaxAngle)).rotated(Vector3(0,1,0), deg2rad(-self.air_direction))


func _input(event):
	if not self.possessed:
		return null

	if event is InputEventMouseMotion and Game.MouseLocked and Game.PlayerInputEnabled:
		self.set_direction(self.direction+(event.relative[0]*-1*MouseSens*MouseSensMultiplyer))
		$SteelCamera.rotation_degrees.x = clamp(($SteelCamera.rotation_degrees.x + event.relative[1]*-1*MouseSens*MouseSensMultiplyer), -90, 90)

		if not is_on_floor():
			if event.relative[0] < 0 and Input.is_action_pressed("MoveLeft"):
				self.airstrafe(event.relative[0])
			elif event.relative[0] > 0 and Input.is_action_pressed("MoveRight"):
				self.airstrafe(event.relative[0])

	elif event is InputEventMouseButton:
		if event.is_pressed():
			if event.button_index == BUTTON_WHEEL_UP:
				self.slot -= 1
				if self.slot < 0:
					self.slot = 9
			elif event.button_index == BUTTON_WHEEL_DOWN:
				self.slot += 1
				if self.slot > 9:
					self.slot = 0
			$SteelHUD.update_hotbar()
