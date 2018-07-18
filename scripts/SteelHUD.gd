extends Node

var player = null


func update_hotbar():
	for index in len(player.inventory):
		print(player.inventory[index])
		get_node('CLayer/HotBarCenter/Vbox').get_children()[index].texture = Items.thumbnail(player.inventory[index]['name'])


func _ready():
	self.player = get_parent()

	self.update_hotbar()
