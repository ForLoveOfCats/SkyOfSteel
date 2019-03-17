import clr

clr.AddReference("GodotSharp")
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
bind("LeftStickUp", "player_input_forward")
bind("S", "player_input_backward")
bind("LeftStickDown", "player_input_backward")
bind("D", "player_input_right")
bind("LeftStickRight", "player_input_right")
bind("A", "player_input_left")
bind("LeftStickLeft", "player_input_left")
bind("Shift", "player_input_sprint")
bind("LeftStickClick", "player_input_sprint")
bind("Space", "player_input_jump")
bind("XboxA", "player_input_jump")
bind("Control", "player_input_crouch")
bind("LeftStickClick", "player_input_crouch")

bind("WheelUp", "player_input_inventory_up")
bind("XboxLB", "player_input_inventory_up")
bind("WheelDown", "player_input_inventory_down")
bind("XboxRB", "player_input_inventory_down")

bind("MouseUp", "player_input_look_up")
bind("RightStickUp", "player_input_look_up")
bind("MouseDown", "player_input_look_down")
bind("RightStickDown", "player_input_look_down")
bind("MouseRight", "player_input_look_right")
bind("RightStickRight", "player_input_look_right")
bind("MouseLeft", "player_input_look_left")
bind("RightStickLeft", "player_input_look_left")

bind("R", "player_input_build_rotate") # Not enough controller buttons for everything, so this one gets excluded :(
bind("Q", "player_input_drop")
bind("XboxX", "player_input_drop")

bind("K", "player_position_reset")
bind("ControllerOverview", "player_position_reset")
bind("T", "fly_toggle")
bind("XboxY", "fly_toggle")

bind("MouseOne", "player_input_primary_fire")
bind("XboxRT", "player_input_primary_fire")
bind("MouseTwo", "player_input_secondary_fire")
bind("XboxLT", "player_input_secondary_fire")

chunk_render_distance(10)
fps_max(200)

