extends VBoxContainer


var is_visible = false


func hide():
	$LineEdit.set_editable(false)
	$LineEdit.text = ''
	.hide()
	self.is_visible = false


func show():
	.show()
	self.is_visible = true
	$LineEdit.set_editable(true)
	$LineEdit.grab_focus()


func console_add_line(string):
	var new_label = load("res://Scripting/ConsoleLine.tscn").instance()
	new_label.text = string
	new_label.set_clip_text(true)
	get_node("HBox/ConsoleScroll/VBox").add_child(new_label)


func log_add_line(string):
	var new_label = load("res://Scripting/ConsoleLine.tscn").instance()
	new_label.text = string
	new_label.set_clip_text(true)
	get_node("HBox/LogScroll/VBox").add_child(new_label)


func _ready():
	self.hide()


func _process(delta):
	if Input.is_action_just_pressed("Enter") and self.is_visible:
		Console.Execute($LineEdit.text)
		$LineEdit.text = ''
