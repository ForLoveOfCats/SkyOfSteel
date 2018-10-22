using Godot;
using System;
using System.Collections.Generic;


public class Bindings : Node
{
	private static List<string> BindingList = new List<string>();

	private static Bindings Self;
	private Bindings()
	{
		Self = this;
	}

	public static void Bind(string FunctionName, int ScanCode)
	{
		if(InputMap.HasAction(FunctionName))
		{
			InputMap.EraseAction(FunctionName);
		}
		InputMap.AddAction(FunctionName);
		InputEventKey Event = new InputEventKey();
		Event.Scancode = ScanCode;
		InputMap.ActionAddEvent(FunctionName, Event);
		BindingList.Add(FunctionName);
	}


	public override void _Process(float Delta)
	{
		foreach(string Binding in BindingList)
		{
			if(Input.IsActionJustPressed(Binding))
			{
				Scripting.ConsoleEngine.CallGlobalFunction(Binding, 1);
			}
			else if(Input.IsActionJustReleased(Binding))
			{
				Scripting.ConsoleEngine.CallGlobalFunction(Binding, 0);
			}
		}
	}
}
