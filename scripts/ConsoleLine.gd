extends Label

func _ready():
	connect("item_rect_changed", self, "on_item_rect_changed")

func on_item_rect_changed():
	set_custom_minimum_size(Vector2(0, (get_line_height()+get_constant("line_spacing")) * get_line_count()))
