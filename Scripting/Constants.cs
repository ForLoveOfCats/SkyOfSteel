using Godot;
using System;
using System.Collections.Generic;

public class Constants : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public const int XBOX_A = 0;
    public const int XBOX_B = 1;
    public const int XBOX_X = 3;
    public const int XBOX_Y = 2;
    
    public const int XBOX_OVERVIEW = 10;
    public const int XBOX_OPTIONS = 11;
    
    public const int XBOX_DPAD_UP = 12;
    public const int XBOX_DPAD_DOWN = 13;
    public const int XBOX_DPAD_LEFT = 14;
    public const int XBOX_DPAD_RIGHT = 15;
    
    public const int XBOX_LB = 4;
    public const int XBOX_RB = 5;
    
    public const int XBOX_LT = 6;
    public const int XBOX_RT = 7;
    
    public const int XBOX_LS_CLICK = 8;
    public const int XBOX_RS_CLICK = 9;
    
    public const int XBOX_LS_HORIZONTAL = 0;
    public const int XBOX_LS_VERTICAL = 1;
    
    public const int XBOX_RS_HORIZONTAL = 2;
    public const int XBOX_RS_VERTICAL = 3;
    
    public const int JOYSTICK_UP = -1;
    public const int JOYSTICK_DOWN = 1;
    public const int JOYSTICK_LEFT = -1;
    public const int JOYSTICK_RIGHT = 1;
    public override void _Ready()
    {
        
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
