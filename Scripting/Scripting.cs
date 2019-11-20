using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Immutable;



public class Scripting : Node
{
	public static Scripting Self;
	Scripting()
	{
		if(Engine.EditorHint) {return;}

		Self = this;
	}


	public static void RunConsoleLine(string Line)
	{}
}
