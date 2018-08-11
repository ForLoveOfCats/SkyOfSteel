extends Node

var player = null


func update_hotbar():
	for index in len(player.inventory):
		var node = get_node('CLayer/HotBarCenter/HBoxContainer/Vbox').get_children()[index]
		if player.inventory[index] != null:
			node.texture = Items.thumbnail(player.inventory[index].name)
		node.rect_min_size = Vector2(get_viewport().size.y/11, get_viewport().size.y/11)

		node = get_node('CLayer/HotBarCenter/HBoxContainer/Vbox2').get_children()[index]
		node.rect_min_size = Vector2(get_viewport().size.y/11, get_viewport().size.y/11)
		node.texture = Items.Alpha

	var node = get_node('CLayer/HotBarCenter/HBoxContainer/Vbox2').get_children()[player.slot]
	node.texture = Items.Triangle
	node.show()


func _on_screen_resized():
	self.update_hotbar()

func _ready():
	self.player = get_parent()
	get_tree().connect("screen_resized", self, "_on_screen_resized")

	self.update_hotbar()
