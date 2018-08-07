extends Node

var player = null


func update_hotbar():
	for index in len(player.inventory):
		var node = get_node('CLayer/HotBarCenter/Vbox').get_children()[index]
		if player.inventory[index] != null:
			node.texture = Items.thumbnail(player.inventory[index].name)
		node.rect_min_size = Vector2(get_viewport().size.y/11, get_viewport().size.y/11)



func _on_screen_resized():
	print(randi())
	self.update_hotbar()

func _ready():
	self.player = get_parent()
	get_tree().connect("screen_resized", self, "_on_screen_resized")

	self.update_hotbar()
