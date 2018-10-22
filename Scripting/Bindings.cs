using Godot;
using System;
using System.Collections.Generic;


public class Bindings : Node
{
	class BindingObject
	{
		public string Name = "";
		public BIND_TYPE Type = Bindings.BIND_TYPE.SCANCODE;

		public BindingObject(string NameArg, BIND_TYPE TypeArg)
		{
			this.Name = NameArg;
			this.Type = TypeArg;
		}
	}


	public enum BIND_TYPE {SCANCODE, MOUSEBUTTON, MOUSEWHEEL, AXIS}
	private static string[] MouseButtonList = {"MouseOne", "MouseTwo", "MouseThree"};
	private static string[] MouseWheelList = {"WheelUp", "WheelDown"};
	private static List<BindingObject> BindingList = new List<BindingObject>();

	private static Bindings Self;
	private Bindings()
	{
		Self = this;
	}

	public static void Bind(string FunctionName, string InputString)
	{
		BIND_TYPE Type = BIND_TYPE.SCANCODE;
		if(System.Array.IndexOf(MouseButtonList, InputString) >= 0)
		{
			Type = BIND_TYPE.MOUSEBUTTON;
		}
		if(System.Array.IndexOf(MouseWheelList, InputString) >= 0)
		{
			Type = BIND_TYPE.MOUSEWHEEL;
		}

		if(InputMap.HasAction(FunctionName))
		{
			InputMap.EraseAction(FunctionName);
		}

		if(Type == BIND_TYPE.SCANCODE)
		{
			InputMap.AddAction(FunctionName);
			InputEventKey Event = new InputEventKey();
			Event.Scancode = OS.FindScancodeFromString(InputString);
			InputMap.ActionAddEvent(FunctionName, Event);
			BindingList.Add(new BindingObject(FunctionName, Type));
		}
		else if(Type == BIND_TYPE.MOUSEBUTTON)
		{
			InputMap.AddAction(FunctionName);
			InputEventMouseButton Event = new InputEventMouseButton();
			switch(InputString)
			{
				case("MouseOne"):
					Event.ButtonIndex = (int)ButtonList.Left;
					break;
				case("MouseTwo"):
					Event.ButtonIndex = (int)ButtonList.Right;
					break;
				case("MouseThree"):
					Event.ButtonIndex = (int)ButtonList.Middle;
					break;
				//No default as this else if will not run unless one of these string will match anyway
			}
			InputMap.ActionAddEvent(FunctionName, Event);
			BindingList.Add(new BindingObject(FunctionName, Type));
		}
		else if(Type == BIND_TYPE.MOUSEWHEEL)
		{
			InputMap.AddAction(FunctionName);
			InputEventMouseButton Event = new InputEventMouseButton();
			switch(InputString)
			{
				case("WheelUp"):
					Event.ButtonIndex = (int)ButtonList.WheelUp;
					break;
				case("WheelDown"):
					Event.ButtonIndex = (int)ButtonList.WheelDown;
					break;
			}
			InputMap.ActionAddEvent(FunctionName, Event);
			BindingList.Add(new BindingObject(FunctionName, Type));
		}
	}


	public static void UnBind(string FunctionName)
	{
		if(InputMap.HasAction(FunctionName))
		{
			InputMap.EraseAction(FunctionName);
		}
	}


	public override void _Process(float Delta)
	{
		foreach(BindingObject Binding in BindingList)
		{
			if(Binding.Type == BIND_TYPE.SCANCODE || Binding.Type == BIND_TYPE.MOUSEBUTTON)
			{
				if(Input.IsActionJustPressed(Binding.Name))
				{
					Scripting.ConsoleEngine.CallGlobalFunction(Binding.Name, 1);
				}
				else if(Input.IsActionJustReleased(Binding.Name))
				{
					Scripting.ConsoleEngine.CallGlobalFunction(Binding.Name, 0);
				}
			}
			else if(Binding.Type == BIND_TYPE.MOUSEWHEEL)
			{
				if(Input.IsActionJustReleased(Binding.Name))
				{
					Scripting.ConsoleEngine.CallGlobalFunction(Binding.Name, 1);
				}
			}
		}
	}
}
