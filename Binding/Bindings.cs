using Godot;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;



public class Bindings : Node
{
	public enum TYPE {UNSET, SCANCODE, MOUSEBUTTON, MOUSEWHEEL, MOUSEAXIS, CONTROLLERBUTTON, CONTROLLERAXIS}
	public enum DIRECTION {UP, DOWN, RIGHT, LEFT};
	private static List<BindingObject> BindingsWithArg = new List<BindingObject>();
	private static List<BindingObject> BindingsWithoutArg = new List<BindingObject>();

	private static Bindings Self;
	private Bindings()
	{
		Self = this;
	}


	public static bool Bind(string KeyName, string FunctionName)
	{
		BindingObject NewBind = new BindingObject(KeyName);

		bool Found = false;
		IEnumerable<MethodInfo> Methods = Assembly.GetExecutingAssembly().GetTypes()
			.SelectMany(t => t.GetMethods())
			.Where(m => m.GetCustomAttributes(typeof(SteelInputWithoutArg), false).Length > 0);
		foreach(MethodInfo Method in Methods)
		{
			if(Method.Name == FunctionName)
			{
				Found = true;
				SteelInputWithoutArg Data = Attribute.GetCustomAttribute(Method, typeof(SteelInputWithoutArg)) as SteelInputWithoutArg;
				NewBind.FuncWithoutArg = Data.Function;
				break;
			}
		}

		Methods = Assembly.GetExecutingAssembly().GetTypes()
			.SelectMany(t => t.GetMethods())
			.Where(m => m.GetCustomAttributes(typeof(SteelInputWithArg), false).Length > 0);
		foreach(MethodInfo Method in Methods)
		{
			if(Method.Name == FunctionName)
			{
				Found = true;
				SteelInputWithArg Data = Attribute.GetCustomAttribute(Method, typeof(SteelInputWithArg)) as SteelInputWithArg;
				NewBind.FuncWithArg = Data.Function;
				break;
			}
		}

		if(!Found)
		{
			Console.ThrowPrint($"The specified function '{FunctionName}' does not exist as a bindable function");
			return false;
		}

		Nullable<ButtonList> ButtonValue = null; //Making it null by default prevents a compile warning further down
		Nullable<DIRECTION> AxisDirection = null; //Making it null by default prevents a compile warning further down
		Nullable<JoystickList> ControllerButtonValue = null; // Making a new variable for Controller buttons because
		uint Scancode = 0;
		switch(KeyName) //Checks custom string literals first then assumes Scancode
		{
			case("MouseOne"): {
				NewBind.Type = TYPE.MOUSEBUTTON;
				ButtonValue = ButtonList.Left;
				break;
			}
			case("MouseTwo"): {
				NewBind.Type = TYPE.MOUSEBUTTON;
				ButtonValue = ButtonList.Right;
				break;
			}
			case("MouseThree"): {
				NewBind.Type = TYPE.MOUSEBUTTON;
				ButtonValue = ButtonList.Middle;
				break;
			}

			case("WheelUp"): {
				NewBind.Type = TYPE.MOUSEWHEEL;
				ButtonValue = ButtonList.WheelUp;
				break;
			}

			case("WheelDown"): {
				NewBind.Type = TYPE.MOUSEWHEEL;
				ButtonValue = ButtonList.WheelDown;
				break;
			}

			case("MouseUp"): {
				NewBind.Type = TYPE.MOUSEAXIS;
				AxisDirection = DIRECTION.UP;
				break;
			}

			case("MouseDown"): {
				NewBind.Type = TYPE.MOUSEAXIS;
				AxisDirection = DIRECTION.DOWN;
				break;
			}

			case("MouseRight"): {
				NewBind.Type = TYPE.MOUSEAXIS;
				AxisDirection = DIRECTION.RIGHT;
				break;
			}

			case("MouseLeft"): {
				NewBind.Type = TYPE.MOUSEAXIS;
				AxisDirection = DIRECTION.LEFT;
				break;
			}

			case("LeftStickUp"): {
				NewBind.Type = TYPE.CONTROLLERAXIS;
				AxisDirection = DIRECTION.UP;
				ControllerButtonValue = JoystickList.AnalogLy;
				break;
			}

			case("LeftStickDown"): {
				NewBind.Type = TYPE.CONTROLLERAXIS;
				AxisDirection = DIRECTION.DOWN;
				ControllerButtonValue = JoystickList.AnalogLy;
				break;
			}

			case("LeftStickLeft"): {
				NewBind.Type = TYPE.CONTROLLERAXIS;
				AxisDirection = DIRECTION.LEFT;
				ControllerButtonValue = JoystickList.AnalogLx;
				break;
			}

			case("LeftStickRight"): {
				NewBind.Type = TYPE.CONTROLLERAXIS;
				AxisDirection = DIRECTION.RIGHT;
				ControllerButtonValue = JoystickList.AnalogLx;
				break;
			}

			case("RightStickUp"): {
				NewBind.Type = TYPE.CONTROLLERAXIS;
				AxisDirection = DIRECTION.UP;
				ControllerButtonValue = JoystickList.AnalogRy;
				break;
			}
			case("RightStickDown"): {
				NewBind.Type = TYPE.CONTROLLERAXIS;
				AxisDirection = DIRECTION.DOWN;
				ControllerButtonValue = JoystickList.AnalogRy;
				break;
			}
			case("RightStickLeft"): {
				NewBind.Type = TYPE.CONTROLLERAXIS;
				AxisDirection = DIRECTION.LEFT;
				ControllerButtonValue = JoystickList.AnalogRx;
				break;
			}
			case("RightStickRight"): {
				NewBind.Type = TYPE.CONTROLLERAXIS;
				AxisDirection = DIRECTION.RIGHT;
				ControllerButtonValue = JoystickList.AnalogRx;
				break;
			}

			case("XboxA"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.XboxA;
				break;
			}

			case("XboxB"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.XboxB;
				break;
			}

			case("XboxX"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.XboxX;
				break;
			}

			case("XboxY"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.XboxY;
				break;
			}

			case("XboxLB"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.L;
				break;
			}

			case("XboxRB"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.R;
				break;
			}

			case("XboxLT"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.L2;
				break;
			}

			case("XboxRT"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.R2;
				break;
			}

			case("RightStickClick"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.R3;
				break;
			}

			case("LeftStickClick"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.L3;
				break;
			}

			case("DPadUp"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.DpadUp;
				break;
			}

			case("DPadDown"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.DpadDown;
				break;
			}

			case("DPadLeft"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.DpadLeft;
				break;
			}

			case("DPadRight"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.DpadRight;
				break;
			}

			case("XboxStart"): {
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.Start;
				break;
			}

			case("XboxSelect"): {
				// Or Select. Or Share. Or The big thing in the middle of ps4 remotes. Or -.
				NewBind.Type = TYPE.CONTROLLERBUTTON;
				ControllerButtonValue = JoystickList.Select;
				break;
			}

			default: {
				//Does not match any custom string literal must either be a Scancode or is invalid
				uint LocalScancode = (uint)OS.FindScancodeFromString(KeyName);
				if(LocalScancode != 0)
				{
					//Is a valid Scancode
					NewBind.Type = TYPE.SCANCODE;
					Scancode = LocalScancode;
				}
				else
				{
					//If not a valid Scancode then the provided key must not be a valid key
					Console.ThrowPrint($"The supplied key '{KeyName}' is not a valid key");
					return false;
				}
				break;
			}
		}
		//Now we have everything we need to setup the bind with Godot's input system

		//First clear any bind with the same key
		UnBind(KeyName);

		//Then add new bind
		InputMap.AddAction(KeyName);
		switch(NewBind.Type)
		{
			case(TYPE.SCANCODE): {
				InputEventKey Event = new InputEventKey();
				Event.Scancode = Scancode;
				InputMap.ActionAddEvent(KeyName, Event);
				break;
			}

			case(TYPE.MOUSEBUTTON):
			case(TYPE.MOUSEWHEEL): {
				InputEventMouseButton Event = new InputEventMouseButton();
				Event.ButtonIndex = (int)ButtonValue;
				InputMap.ActionAddEvent(KeyName, Event);
				break;
			}

			case(TYPE.MOUSEAXIS): {
				InputEventMouseMotion Event = new InputEventMouseMotion();
				InputMap.ActionAddEvent(KeyName, Event);
				NewBind.AxisDirection = (DIRECTION)AxisDirection; //Has to cast as it is Nullable
				break;
			}

			case(TYPE.CONTROLLERAXIS): {
				InputEventJoypadMotion Event = new InputEventJoypadMotion();
				Event.Axis = (int)ControllerButtonValue; // Set which Joystick axis we're using
				switch (AxisDirection) { // Set which direction on the axis we need to trigger the event
					case(DIRECTION.UP): {
						Event.AxisValue = -1; // -1, on the Vertical axis is up
						break;
					}

					case(DIRECTION.LEFT): {
						Event.AxisValue = -1; // -1, on the Horizontal axis is left
						break;
					}

					case(DIRECTION.DOWN): {
						Event.AxisValue = 1; // 1, on the Vertical axis is down
						break;
					}

					case(DIRECTION.RIGHT): {
						Event.AxisValue = 1; // 1, on the Horizontal axis is right
						break;
					}
				}

				InputMap.ActionAddEvent(KeyName, Event);
				NewBind.AxisDirection = (DIRECTION)AxisDirection; //Has to cast as it is Nullable
				break;
			}

			case(TYPE.CONTROLLERBUTTON): {
				InputEventJoypadButton Event = new InputEventJoypadButton();
				Event.ButtonIndex = (int)ControllerButtonValue;
				InputMap.ActionAddEvent(KeyName, Event);
				break;
			}
		}

		if(NewBind.FuncWithArg != null)
		{
			BindingsWithArg.Add(NewBind);
		}
		else if(NewBind.FuncWithoutArg != null)
		{
			BindingsWithoutArg.Add(NewBind);
		}

		return true;
	}


	public static void UnBind(string KeyName)
	{
		if(InputMap.HasAction(KeyName))
		{
			InputMap.EraseAction(KeyName);

			List<BindingObject> Removing = new List<BindingObject>();
			foreach(BindingObject Bind in BindingsWithArg)
			{
				if(Bind.Name == KeyName)
				{
					Removing.Add(Bind);
				}
			}
			foreach(BindingObject Bind in Removing)
			{
				BindingsWithArg.Remove(Bind);
			}

			Removing.Clear();

			foreach(BindingObject Bind in BindingsWithoutArg)
			{
				if(Bind.Name == KeyName)
				{
					Removing.Add(Bind);
				}
			}
			foreach(BindingObject Bind in Removing)
			{
				BindingsWithoutArg.Remove(Bind);
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


	public override void _PhysicsProcess(float Delta)
	{
		if(!Game.BindsEnabled)
		{
			return;
		}

		foreach(BindingObject Binding in BindingsWithArg)
		{
			if(Binding.Type == TYPE.SCANCODE || Binding.Type == TYPE.MOUSEBUTTON || Binding.Type == TYPE.CONTROLLERBUTTON)
			{
				if(Input.IsActionJustPressed(Binding.Name))
				{
					Binding.FuncWithArg.Invoke(1);
				}
				else if(Input.IsActionJustReleased(Binding.Name))
				{
					Binding.FuncWithArg.Invoke(0);
				}
			}
			else if(Binding.Type == TYPE.MOUSEWHEEL)
			{
				if(Input.IsActionJustReleased(Binding.Name))
				{
					Binding.FuncWithArg.Invoke(1);
				}
			}
			else if(Binding.Type == TYPE.CONTROLLERAXIS)
			{
				int VerticalAxis = 0;
				int HorizontalAxis = 0;
				InputEventJoypadMotion StickEvent = null;

				foreach(InputEvent Option in InputMap.GetActionList(Binding.Name)) {
					if (Option is InputEventJoypadMotion JoyEvent) {
						StickEvent = JoyEvent;
					}
				}

				if (StickEvent.Axis == 0 || StickEvent.Axis == 1)
				{
					// We are using Left stick
					VerticalAxis = 1;
					HorizontalAxis = 0;
				}
				else if (StickEvent.Axis == 2 || StickEvent.Axis == 3)
				{
					// We are using Right stick
					VerticalAxis = 3;
					HorizontalAxis = 2;
				}
				else
				{
					Console.ThrowPrint("This joystick doesn't exist! ?????????");
				}

				if (Math.Abs(Input.GetJoyAxis(0,HorizontalAxis)) >= Game.Deadzone || Math.Abs(Input.GetJoyAxis(0,VerticalAxis)) >= Game.Deadzone)
				{
					float HorizontalMovement = Input.GetJoyAxis(0,HorizontalAxis);
					float VerticalMovement = Input.GetJoyAxis(0,VerticalAxis);
					switch(Binding.AxisDirection)
					{
						case(DIRECTION.UP):
							Binding.FuncWithArg.Invoke(-VerticalMovement);
							break;
						case(DIRECTION.DOWN):
							Binding.FuncWithArg.Invoke(VerticalMovement);
							break;
						case(DIRECTION.RIGHT):
							Binding.FuncWithArg.Invoke(HorizontalMovement);
							break;
						case(DIRECTION.LEFT):
							Binding.FuncWithArg.Invoke(-HorizontalMovement);
							break;
					}
					Binding.JoyWasInDeadzone = false;
				}
				else // Set sens to zero to simulate key release
				{
					if (Binding.JoyWasInDeadzone == false) // Only do this if the Binding wasn't zero last time
					{
						Binding.FuncWithArg.Invoke(0);
						Binding.JoyWasInDeadzone = true;
					}
				}
			}
		}

		foreach(BindingObject Binding in BindingsWithoutArg)
		{
			if(Binding.Type == TYPE.SCANCODE || Binding.Type == TYPE.MOUSEBUTTON || Binding.Type == TYPE.CONTROLLERBUTTON)
			{
				if(Input.IsActionJustPressed(Binding.Name))
				{
					Binding.FuncWithoutArg.Invoke();
				}
			}
			else if(Binding.Type == TYPE.MOUSEWHEEL)
			{
				if(Input.IsActionJustReleased(Binding.Name))
				{
					Binding.FuncWithoutArg.Invoke();
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
				if(Binding.Type == TYPE.MOUSEAXIS)
				{
					switch(Binding.AxisDirection)
					{
						case(DIRECTION.UP):
							Binding.FuncWithArg.Invoke(GreaterEqualZero(-MotionEvent.Relative.y)/Game.LookSensitivity);
							break;
						case(DIRECTION.DOWN):
							Binding.FuncWithArg.Invoke(GreaterEqualZero(MotionEvent.Relative.y)/Game.LookSensitivity);
							break;
						case(DIRECTION.RIGHT):
							Binding.FuncWithArg.Invoke(GreaterEqualZero(MotionEvent.Relative.x)/Game.LookSensitivity);
							break;
						case(DIRECTION.LEFT):
							Binding.FuncWithArg.Invoke(GreaterEqualZero(-MotionEvent.Relative.x)/Game.LookSensitivity);
							break;
					}
				}
			}

			foreach(BindingObject Binding in BindingsWithoutArg)
			{
				if(Binding.Type == TYPE.MOUSEAXIS)
				{
					//Don't need to switch on the direction as it doesn't want an argument anyway
					Binding.FuncWithoutArg.Invoke();
				}
			}
		}
	}
}
