using Godot;
using System;
using System.Collections.Generic;
using Jurassic;


public class Scripting : Node
{
	private static Jurassic.ScriptEngine ServerGmEngine;
	private static Jurassic.ScriptEngine ClientGmEngine;
	private static Jurassic.ScriptEngine ConsoleEngine;


	private static Scripting Self;
	Scripting()
	{
		Self = this;
		ServerGmEngine = new Jurassic.ScriptEngine();
		foreach(List<object> List in API.Expose(API.LEVEL.SERVER_GM, this))
		{
			ServerGmEngine.SetGlobalFunction((string)List[0], (Delegate)List[1]);
		}

		ClientGmEngine = new Jurassic.ScriptEngine();
		foreach(List<object> List in API.Expose(API.LEVEL.CLIENT_GM, this))
		{
			ClientGmEngine.SetGlobalFunction((string)List[0], (Delegate)List[1]);
		}

		ConsoleEngine = new Jurassic.ScriptEngine();
		foreach(List<object> List in API.Expose(API.LEVEL.ADMIN, this))
		{
			ConsoleEngine.SetGlobalFunction((string)List[0], (Delegate)List[1]);
		}
	}


	public override void _Ready()
	{
		File Autoexec = new File();
		if(Autoexec.FileExists("user://autoexec.js"))
		{
			Autoexec.Open("user://autoexec.js", 1);
			Console.Print("Autoexec loaded 'autoexec.js'");
			try
			{
				ConsoleEngine.Execute(Autoexec.GetAsText());
			}
			catch(Exception Error)
			{
				Console.Print(Error.Message);
				Console.Print("AUTOEXEC FAILED: Not all parts of the autoexec executed successfully. It is highly recommended that you fix your autoexec and restart the game.");
			}
		}
		else
		{
			Console.Print("Autoexec not found 'autoexec.js'");
		}
	}


	public static void RunConsoleLine(string Line)
	{
		object Returned;
		try
		{
			Returned = ConsoleEngine.Evaluate(Line);
		}
		catch(Exception Error)
		{
			Console.Print(Error.Message);
			return;
		}

		if(!(Returned is Jurassic.Undefined))
		{
			Console.Print(Returned.ToString());
		}
	}
}
