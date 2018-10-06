using Godot;
using System;
using System.Collections.Generic;
using Jurassic;


public class Scripting : Node
{
	private Jurassic.ScriptEngine ServerGmEngine;
	private Jurassic.ScriptEngine ClientGmEngine;
	private Jurassic.ScriptEngine ConsoleEngine;

	private Node Game = null;
	private Node Console = null;

	Scripting()
	{
		this.ServerGmEngine = new Jurassic.ScriptEngine();
		foreach(List<object> List in API.Expose(API.LEVEL.SERVER_GM, this))
		{
			this.ServerGmEngine.SetGlobalFunction((string)List[0], (Delegate)List[1]);
		}

		this.ClientGmEngine = new Jurassic.ScriptEngine();
		foreach(List<object> List in API.Expose(API.LEVEL.CLIENT_GM, this))
		{
			this.ClientGmEngine.SetGlobalFunction((string)List[0], (Delegate)List[1]);
		}

		this.ConsoleEngine = new Jurassic.ScriptEngine();
		foreach(List<object> List in API.Expose(API.LEVEL.ADMIN, this))
		{
			this.ConsoleEngine.SetGlobalFunction((string)List[0], (Delegate)List[1]);
		}
	}


	public override void _Ready()
	{
		this.Game = GetNode("/root/Game");
		this.Console = GetNode("/root/Console");

		File Autoexec = new File();
		if(Autoexec.FileExists("user://autoexec.js"))
		{
			Autoexec.Open("user://autoexec.js", 1);
			ApiPrint("Autoexec loaded 'autoexec.js'");
			try
			{
				this.ConsoleEngine.Execute(Autoexec.GetAsText());
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


	public void ApiPrint(string ToPrint)
	{
		Console.Call("printf", new string[] {ToPrint});
	}

	public void ApiLog(string ToLog)
	{
		Console.Call("logf", new string[] {ToLog});
	}


	public void RunConsoleLine(string Line)
	{
		object Returned;
		try
		{
			Returned = this.ConsoleEngine.Evaluate(Line);
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
