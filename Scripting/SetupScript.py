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
import World
import Menu
import Startup


bind("W", "player_input_forward")
bind("S", "player_input_backward")
bind("D", "player_input_right")
bind("A", "player_input_left")
bind("Shift", "player_input_sprint")
bind("Space", "player_input_jump")
bind("Control", "player_input_crouch")

bind("WheelUp", "player_input_inventory_up")
bind("WheelDown", "player_input_inventory_down")

bind("MouseUp", "player_input_look_up")
bind("MouseDown", "player_input_look_down")
bind("MouseRight", "player_input_look_right")
bind("MouseLeft", "player_input_look_left")

bind("R", "player_input_build_rotate")
bind("Q", "player_input_drop")

bind("K", "player_position_reset")
bind("T", "fly_toggle")

bind("MouseOne", "player_input_primary_fire")
bind("MouseTwo", "player_input_secondary_fire")

chunk_render_distance(10)
fps_max(200)
