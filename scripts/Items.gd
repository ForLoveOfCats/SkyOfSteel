extends Node


var SteelGame = null


func return_instance(name):
	return {'name':name, 'temp':0, 'count':0, 'uses':0}

func lookup(name):
	var out = {'name':'error', 'type':'error', 'description':'This is an error item and should not exist.'}
	match name:
		'platform':
			out['name'] = 'platform'
			out['type'] = 'structure'
			out['description'] = 'Basic flat platform, for use as a foor or ceiling.'

	return out

func thumbnail(name):
	var out = load("res://textures/thumbnails/" + name + '.png')

	if out == null:
		out = load("res://textures/error.png")
		SConsole.logf('Error: Failed to find thumbnail for item "' + name + '"')

	return out

func _ready():
	self.SteelGame = get_tree().get_root().get_node("SteelGame")
