using Godot;
using System;
using System.Collections.Generic;
using Jurassic;


public class Scripting : Node
{
	private static Jurassic.ScriptEngine ServerGmEngine;
	private static Jurassic.ScriptEngine ClientGmEngine;
	private static Jurassic.ScriptEngine ConsoleEngine;

	private static Node Console = null;
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
		Console = GetNode("/root/Console");

		File Autoexec = new File();
		if(Autoexec.FileExists("user://autoexec.js"))
		{
			Autoexec.Open("user://autoexec.js", 1);
			ApiPrint("Autoexec loaded 'autoexec.js'");
			try
			{
				ConsoleEngine.Execute(Autoexec.GetAsText());
			}
			catch(Exception Error)
			{
				ApiPrint(Error.Message);
				ApiPrint("AUTOEXEC FAILED: Not all parts of the autoexec executed successfully. It is highly recommended that you fix your autoexec and restart the game.");
			}
		}
		else
		{
			ApiPrint("Autoexec not found 'autoexec.js'");
		}
	}


	public static void ApiPrint(string ToPrint)
	{
		Console.Call("printf", new string[] {ToPrint});
	}

	public static void ApiLog(string ToLog)
	{
		Console.Call("logf", new string[] {ToLog});
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
			ApiPrint(Error.Message);
			return;
		}

		if(!(Returned is Jurassic.Undefined))
		{
			ApiPrint(Returned.ToString());
		}
	}
}
