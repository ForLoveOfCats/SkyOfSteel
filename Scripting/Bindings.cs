using Godot;
using System;
using System.Collections.Generic;


public class Bindings : Node
{
	public enum BIND_TYPE {SCANCODE, MOUSEBUTTON, MOUSEWHEEL, AXIS}
	private static List<string> MouseButtonList = new List<string>{"MouseOne", "MouseTwo", "MouseThree"};
	private static List<string> MouseWheelList = new List<string>{"WheelUp", "WheelDown"};
	private static List<string> AxisList = new List<string>{"MouseUp", "MouseDown", "MouseRight", "MouseLeft"};
	private static List<BindingObject> BindingsWithArg = new List<BindingObject>();
	private static List<BindingObject> BindingsWithoutArg = new List<BindingObject>();

	private static Bindings Self;
	private Bindings()
	{
		Self = this;
	}


	public static void Bind(string InputString, string FunctionName)
	{
		dynamic Variable;
		Scripting.ConsoleScope.TryGetVariable(FunctionName, out Variable);
		if(Variable == null || !(Variable is Delegate || Variable is IronPython.Runtime.PythonFunction))
		{
			Console.ThrowPrint($"'{FunctionName}' is not a valid function");
			return;
		}

		Nullable<int> ArgCount = null; //null for the sanity check
		if(Variable is Delegate)
		{
			ArgCount = (Variable as Delegate).Method.GetParameters().Length;
		}
		else if(Variable is IronPython.Runtime.PythonFunction)
		{
			ArgCount = Scripting.ConsoleEngine.Execute($"len({FunctionName}.func_code.co_varnames)");
		}

		if(ArgCount == null)
		{
			//Sanity check
			Console.ThrowPrint($"Cannot find argument count of '{FunctionName}', please contact the developers'");
			return;
		}

		if(ArgCount != 0 && ArgCount != 1)
		{
			Console.ThrowPrint($"Function '{FunctionName}' must take either one or two arguments");
			return;
		}
		if(ArgCount == 1 && Variable is Delegate)
		{
			//TODO Update this once we move to floats instead of floats
			if(!((Delegate)Variable).Method.GetParameters()[0].ParameterType.IsInstanceOfType(new float()))
			{
				Console.ThrowPrint($"Builtin command '{FunctionName}' has a single non-float argument");
				return;
			}
		}

		BIND_TYPE Type = BIND_TYPE.SCANCODE;
		if(MouseButtonList.Contains(InputString))
		{
			Type = BIND_TYPE.MOUSEBUTTON;
		}
		if(MouseWheelList.Contains(InputString))
		{
			Type = BIND_TYPE.MOUSEWHEEL;
		}
		if(AxisList.Contains(InputString))
		{
			Type = BIND_TYPE.AXIS;
		}

		if(InputMap.HasAction(FunctionName))
		{
			InputMap.EraseAction(FunctionName);
			foreach(BindingObject Bind in BindingsWithArg)
			{
				if(Bind.Name == FunctionName)
				{
					//Does not throw exception when not found
					BindingsWithArg.Remove(Bind);
					BindingsWithoutArg.Remove(Bind);
					break;
				}
			}
		}

		if(Type == BIND_TYPE.SCANCODE)
		{
			InputMap.AddAction(FunctionName);
			InputEventKey Event = new InputEventKey();
			Event.Scancode = OS.FindScancodeFromString(InputString);
			InputMap.ActionAddEvent(FunctionName, Event);

			if(ArgCount == 1)
			{
				BindingsWithArg.Add(new BindingObject(FunctionName, Type));
			}
			else if(ArgCount == 0)
			{
				BindingsWithoutArg.Add(new BindingObject(FunctionName, Type));
			}
			else
			{
				Console.ThrowPrint($"Cannot add SCANCODE bind, '{FunctionName}' has an unsuported number of arguments");
			}
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

			if(ArgCount == 1)
			{
				BindingsWithArg.Add(new BindingObject(FunctionName, Type));
			}
			else if(ArgCount == 0)
			{
				BindingsWithoutArg.Add(new BindingObject(FunctionName, Type));
			}
			else
			{
				Console.ThrowPrint($"Cannot add MOUSEBUTTON bind, '{FunctionName}' has an unsuported number of arguments");
			}
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

			if(ArgCount == 1)
			{
				BindingsWithArg.Add(new BindingObject(FunctionName, Type));
			}
			else if(ArgCount == 0)
			{
				BindingsWithoutArg.Add(new BindingObject(FunctionName, Type));
			}
			else
			{
				Console.ThrowPrint($"Cannot add MOUSEWHEEL bind, '{FunctionName}' has an unsuported number of arguments");
			}
		}
		else if(Type == BIND_TYPE.AXIS)
		{
			InputMap.AddAction(FunctionName);
			InputEventMouseMotion Event = new InputEventMouseMotion();
			InputMap.ActionAddEvent(FunctionName, Event);
			BindingObject Bind = new BindingObject(FunctionName, Type);
			switch(InputString)
			{
				case("MouseUp"):
					Bind.AxisDirection = BindingObject.DIRECTION.UP;
					break;
				case("MouseDown"):
					Bind.AxisDirection = BindingObject.DIRECTION.DOWN;
					break;
				case("MouseRight"):
					Bind.AxisDirection = BindingObject.DIRECTION.RIGHT;
					break;
				case("MouseLeft"):
					Bind.AxisDirection = BindingObject.DIRECTION.LEFT;
					break;
			}
			BindingsWithArg.Add(Bind);
		}
	}


	public static void UnBind(string FunctionName)
	{
		if(InputMap.HasAction(FunctionName))
		{
			InputMap.EraseAction(FunctionName);
			foreach(BindingObject Bind in BindingsWithArg)
			{
				if(Bind.Name == FunctionName)
				{
					//Does not throw exception when not found
					BindingsWithArg.Remove(Bind);
					BindingsWithoutArg.Remove(Bind);
					break;
				}
			}
		}
	}


	private static float GreaterEqualZero(float In)
	{
		if(In < 0f)
		{
			return 0f;
		}
		return In;
	}


	public override void _Process(float Delta)
	{
		if(!Game.BindsEnabled)
		{
			return;
		}

		foreach(BindingObject Binding in BindingsWithArg)
		{
			if(Binding.Type == BIND_TYPE.SCANCODE || Binding.Type == BIND_TYPE.MOUSEBUTTON)
			{
				if(Input.IsActionJustPressed(Binding.Name))
				{
					Scripting.ConsoleEngine.Execute($"{Binding.Name}(1)", Scripting.ConsoleScope);
				}
				else if(Input.IsActionJustReleased(Binding.Name))
				{
					Scripting.ConsoleEngine.Execute($"{Binding.Name}(0)", Scripting.ConsoleScope);
				}
			}
			else if(Binding.Type == BIND_TYPE.MOUSEWHEEL)
			{
				if(Input.IsActionJustReleased(Binding.Name))
				{
					Scripting.ConsoleEngine.Execute($"{Binding.Name}(1)", Scripting.ConsoleScope);
				}
			}
		}

		foreach(BindingObject Binding in BindingsWithoutArg)
		{
			if(Binding.Type == BIND_TYPE.SCANCODE || Binding.Type == BIND_TYPE.MOUSEBUTTON)
			{
				if(Input.IsActionJustPressed(Binding.Name))
				{
					Scripting.ConsoleEngine.Execute($"{Binding.Name}()", Scripting.ConsoleScope);
				}
			}
			else if(Binding.Type == BIND_TYPE.MOUSEWHEEL)
			{
				if(Input.IsActionJustReleased(Binding.Name))
				{
					Scripting.ConsoleEngine.Execute($"{Binding.Name}()", Scripting.ConsoleScope);
				}
			}
		}
	}


	public override void _Input(InputEvent Event)
	{
		if(!Game.BindsEnabled)
		{
			return;
		}

		if(Event is InputEventMouseMotion MotionEvent)
		{
			foreach(BindingObject Binding in BindingsWithArg)
			{
				if(Binding.Type == BIND_TYPE.AXIS)
				{
					switch(Binding.AxisDirection)
					{
						case(BindingObject.DIRECTION.UP):
							Scripting.ConsoleEngine.Execute($"{Binding.Name}({(float)new decimal (GreaterEqualZero(MotionEvent.Relative.y*-1))})", Scripting.ConsoleScope);
							break;
						case(BindingObject.DIRECTION.DOWN):
							Scripting.ConsoleEngine.Execute($"{Binding.Name}({(float)new decimal (GreaterEqualZero(MotionEvent.Relative.y))})", Scripting.ConsoleScope);
							break;
						case(BindingObject.DIRECTION.RIGHT):
							Scripting.ConsoleEngine.Execute($"{Binding.Name}({(float)new decimal (GreaterEqualZero(MotionEvent.Relative.x))})", Scripting.ConsoleScope);
							break;
						case(BindingObject.DIRECTION.LEFT):
							Scripting.ConsoleEngine.Execute($"{Binding.Name}({(float)new decimal (GreaterEqualZero(MotionEvent.Relative.x*-1))})", Scripting.ConsoleScope);
							break;
					}
				}
			}

			foreach(BindingObject Binding in BindingsWithoutArg)
			{
				if(Binding.Type == BIND_TYPE.AXIS)
				{
					Scripting.ConsoleEngine.Execute($"{Binding.Name}()", Scripting.ConsoleScope);
				}
			}
		}
	}
}
