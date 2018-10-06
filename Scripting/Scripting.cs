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

		this.ClientGmEngine = new Jurassic.ScriptEngine();

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
