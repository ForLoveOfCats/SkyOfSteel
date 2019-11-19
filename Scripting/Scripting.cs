using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Immutable;
using Sc = Microsoft.CodeAnalysis.Scripting;
using Cs = Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript;


public class Scripting : Node
{
	public static Sc.ScriptOptions ScriptOptions;

	public static Sc.ScriptState ConsoleState;

	public static Scripting Self;
	Scripting()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		ScriptOptions = Sc.ScriptOptions.Default.WithReferences(AppDomain.CurrentDomain.GetAssemblies())
			.AddReferences(Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject)),  // System.Code
						   Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),  // Microsoft.CSharp
						   Assembly.GetAssembly(typeof(System.Dynamic.ExpandoObject)));  // System.Dynamic

		Sc.Script CEngine = Cs.Create("", ScriptOptions);
		ConsoleState = CEngine.ContinueWith("using System; using System.Dynamic; using Godot; using static API; using static Items.ID;")
			.RunAsync().Result;
	}


	public static void RunConsoleLine(string Line)
	{
		object Returned = null;

		try
		{
			ConsoleState = ConsoleState.ContinueWithAsync(Line).Result;
			Returned = ConsoleState.ReturnValue as object; //just in case of an issue this should become null
		}
		catch(Exception Err)
		{
			Console.Print(Err.Message);
		}

		if(Returned != null)
		{
			Console.Print(Returned);
		}
	}
}
