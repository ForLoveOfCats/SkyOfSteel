using Godot;
using System;
using System.Collections.Generic;
using Jurassic;


public class Scripting : Node
{
	private static Jurassic.ScriptEngine ServerGmEngine;
	private static Jurassic.ScriptEngine ClientGmEngine;
	public static Jurassic.ScriptEngine ConsoleEngine;

	private static Scripting Self;
	Scripting()
	{
		Self = this;

		ConsoleEngine = new Jurassic.ScriptEngine();
		foreach(List<object> List in API.Expose(API.LEVEL.ADMIN, this))
		{
			ConsoleEngine.SetGlobalFunction((string)List[0], (Delegate)List[1]);
		}

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
	}


	public override void _Ready()
	{
		File SetupScript = new File();
		SetupScript.Open("res://Scripting/SetupScript.js", 1);
		ConsoleEngine.Execute(SetupScript.GetAsText());
		SetupScript.Close();

		File Autoexec = new File();
		if(Autoexec.FileExists("user://autoexec.js"))
		{
			Autoexec.Open("user://autoexec.js", 1);
			Console.Print("Autoexec loaded 'autoexec.js'");
			try
			{
				ConsoleEngine.Execute(Autoexec.GetAsText());
			}
			catch(JavaScriptException Error)
			{
				Console.Print(Error.Message + " @ line " + Error.LineNumber.ToString());
				Console.Print("AUTOEXEC FAILED: Not all parts of the autoexec executed successfully. It is highly recommended that you fix your autoexec and restart the game.");
			}
		}
		else
		{
			Console.Print("Autoexec not found 'autoexec.js'");
		}

		string[] CmdArgs = OS.GetCmdlineArgs();
		foreach(string CurrentArg in CmdArgs)
		{
			Console.Log("Command line argument '" + CurrentArg + "'");
			if(CurrentArg == "-dev_connect")
			{
				Console.Execute("connect();");
			}
		}
	}


	public static void RunConsoleLine(string Line)
	{
		object Returned;
		try
		{
			Returned = ConsoleEngine.Evaluate(Line);
		}
		catch(JavaScriptException Error)
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
