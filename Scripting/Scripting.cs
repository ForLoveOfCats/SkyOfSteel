using Godot;
using System;
using System.Collections.Generic;
using Jurassic;


public class Scripting : Node
{
	public static Jurassic.ScriptEngine ServerGmEngine;
	private static Jurassic.ScriptEngine ClientGmEngine;
	public static Jurassic.ScriptEngine ConsoleEngine;

	public static string GamemodeName;

	private static Scripting Self;
	Scripting()
	{
		Self = this;

		ConsoleEngine = new Jurassic.ScriptEngine();
		foreach(List<object> List in API.Expose(API.LEVEL.CONSOLE, this))
		{
			ConsoleEngine.SetGlobalFunction((string)List[0], (Delegate)List[1]);
		}

		SetupServerEngine();

		ClientGmEngine = new Jurassic.ScriptEngine();
		foreach(List<object> List in API.Expose(API.LEVEL.CLIENT_GM, this))
		{
			ClientGmEngine.SetGlobalFunction((string)List[0], (Delegate)List[1]);
		}
	}


	public static object ToJs(object ToConvert)
	{
		if(ToConvert is Vector3)
		{
			Vector3 Vec = (Vector3)ToConvert;
			Jurassic.Library.ArrayInstance ArrVec = Scripting.ConsoleEngine.Array.Construct();
			ArrVec.Push((double)Vec.x);
			ArrVec.Push((double)Vec.y);
			ArrVec.Push((double)Vec.z);
			return ArrVec;
		}

		if(ToConvert is float)
		{
			return Convert.ToDouble((float)ToConvert);
		}

		return Jurassic.Undefined.Value;
	}


	public static object[] ToJs(object[] ConvertArray)
	{
		object[] Out = new object[ConvertArray.Length];
		int Iteration = 0;
		foreach(object ToConvert in ConvertArray)
		{
			Out[Iteration] = ToJs(ToConvert);
			Iteration += 1;
		}
		return Out;
	}


	public static void SetupServerEngine()
	{
		ServerGmEngine = new Jurassic.ScriptEngine();
		foreach(List<object> List in API.Expose(API.LEVEL.SERVER_GM, Self))
		{
			ServerGmEngine.SetGlobalFunction((string)List[0], (Delegate)List[1]);
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
		Autoexec.Close();

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


	public override void _PhysicsProcess(float Delta)
	{
		try
		{
			ConsoleEngine.CallGlobalFunction("_tick", new object[] {(double)Delta});
		}
		catch(System.InvalidOperationException){} //This just means that _tick is not a delcared function
		catch(JavaScriptException Error)
		{
			Console.Print(Error.Message);
		}
	}


	public static void LoadGameMode(string Name)
	{
		Directory ModeDir = new Directory();
		if(ModeDir.DirExists("user://GameModes/" + Name)) //Gamemode exists
		{
			GamemodeName = Name;

			if(ModeDir.FileExists("user://GameModes/" + Name + "/Server.js")) //Has a server side script
			{
				SetupServerEngine();
				File ServerScript = new File();
				ServerScript.Open("user://GameModes/" + Name + "/Server.js", 1);
				ServerGmEngine.Execute(ServerScript.GetAsText());
				ServerScript.Close();
			}

			if(ModeDir.FileExists("user://GameModes/" + Name + "/Client.js")) //Has a client side script
			{
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
