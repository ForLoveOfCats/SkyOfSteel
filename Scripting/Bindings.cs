using Godot;
using System;
using System.Collections.Generic;


public class Bindings : Node
{
	class Binding
	{
		public int ScanCode = (int)KeyList.Unknown;
		public string FunctionName = "";

		public Binding(string FunctionNameArg, int ScanCodeArg)
		{
			this.ScanCode = ScanCodeArg;
			this.FunctionName = FunctionNameArg;
		}
	}


	private static List<Binding> BindingList = new List<Binding>();

	private static Bindings Self;
	private Bindings()
	{
		Self = this;
	}


	public static void Bind(string FunctionName, int ScanCode)
	{
		BindingList.Add(new Binding(FunctionName, ScanCode));
	}


	public override void _Input(InputEvent Event)
	{
		if(Event is InputEventKey EventKey)
		{
			foreach(Binding Binding in BindingList)
			{
				if(Binding.ScanCode == EventKey.Scancode)
				{
					if(EventKey.IsPressed())
					{
						Scripting.ConsoleEngine.CallGlobalFunction(Binding.FunctionName, 1);
					}
					else
					{
						Scripting.ConsoleEngine.CallGlobalFunction(Binding.FunctionName, 0);
					}
				}
			}
		}
	}
}
