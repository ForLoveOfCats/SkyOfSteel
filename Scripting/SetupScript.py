import clr

clr.AddReference("GodotSharp");
import Godot

clr.AddReference("SkyOfSteel")
import Game
import Console
import Scripting
import Net
import Bindings
import Items
import Building
import Menu
import Startup
import Constants


#bind("player_input_forward", "W")
bind_controller_axis("player_input_forward", "W", Constants.XBOX_LS_VERTICAL, Constants.JOYSTICK_UP)
#bind("player_input_backward", "S")
bind_controller_axis("player_input_backward", "S", Constants.XBOX_LS_VERTICAL, Constants.JOYSTICK_DOWN)
#bind("player_input_right", "D")
bind_controller_axis("player_input_right", "D", Constants.XBOX_LS_HORIZONTAL, Constants.JOYSTICK_RIGHT)
#bind("player_input_left", "A")
bind_controller_axis("player_input_left", "A", Constants.XBOX_LS_HORIZONTAL, Constants.JOYSTICK_LEFT)
#bind("player_input_sprint", "Shift")


#bind("player_input_jump", "Space")

bind_controller_button("player_input_jump","Space",Constants.XBOX_A)
#bind("player_input_crouch", "Control")

bind_controller_button("player_input_crouch","Control",Constants.XBOX_RS_CLICK)

#bind("player_input_inventory_up", "WheelUp")
bind_controller_button("player_input_inventory_up","WheelUp",Constants.XBOX_LB)
#bind("player_input_inventory_down", "WheelDown")
bind_controller_button("player_input_inventory_down","WheelDown",Constants.XBOX_RB)

#bind("player_input_look_up", "MouseUp")
bind_controller_axis("player_input_look_up", "MouseUp", Constants.XBOX_RS_VERTICAL, Constants.JOYSTICK_UP)
#bind("player_input_look_down", "MouseDown")
bind_controller_axis("player_input_look_down", "MouseDown", Constants.XBOX_RS_VERTICAL, Constants.JOYSTICK_DOWN)
#bind("player_input_look_right", "MouseRight")
bind_controller_axis("player_input_look_right", "MouseRight", Constants.XBOX_RS_HORIZONTAL, Constants.JOYSTICK_RIGHT)
#bind("player_input_look_left", "MouseLeft")
bind_controller_axis("player_input_look_left", "MouseLeft", Constants.XBOX_RS_HORIZONTAL, Constants.JOYSTICK_LEFT)

bind("player_input_build_rotate", "R")
bind("player_input_drop", "Q")

#bind("player_position_reset", "K")
bind_controller_button("player_position_reset","K",Constants.XBOX_Y)
#bind("fly_toggle", "T")
bind_controller_button("fly_toggle","T",Constants.XBOX_LS_CLICK)

#bind("player_input_primary_fire", "MouseOne")
bind_controller_button("player_input_primary_fire","MouseOne",Constants.XBOX_RT)
#bind("player_input_secondary_fire", "MouseTwo")
bind_controller_button("player_input_secondary_fire","MouseTwo",Constants.XBOX_LT)


chunk_render_distance(10)
fps_max(200)
