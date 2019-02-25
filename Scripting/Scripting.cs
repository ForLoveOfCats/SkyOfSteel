using Godot;
using System;
using System.Collections.Generic;
using Jurassic;
using Jurassic.Library;


public class Scripting : Node
{
	public static Jurassic.ScriptEngine ServerGmEngine;
	public static Jurassic.ScriptEngine ClientGmEngine;
	public static Jurassic.ScriptEngine ConsoleEngine;

	public static string GamemodeName;
	public static string ClientGmScript;

	public static Scripting Self;
	Scripting()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		ConsoleEngine = new Jurassic.ScriptEngine();
		foreach(List<object> List in API.Expose(API.LEVEL.CONSOLE, this))
		{
			ConsoleEngine.SetGlobalFunction((string)List[0], (Delegate)List[1]);
		}
		foreach(List<object> List in API.ExposeConstructors(API.LEVEL.CONSOLE))
		{
			ConsoleEngine.SetGlobalValue((string)List[0], (ClrFunction)List[1]);
		}

		SetupServerEngine();
		SetupClientEngine();
	}


	public static object ToJs(object ToConvert)
	{
		if(ToConvert is Vector3)
		{
			Vector3 Vec = (Vector3)ToConvert;
			return new JsVector3(Scripting.ConsoleEngine.Object.InstancePrototype, (double)Vec.x, (double)Vec.y, (double)Vec.z);
		}

		if(ToConvert is float)
		{
			return Convert.ToDouble((float)ToConvert);
		}

		if(ToConvert is int)
		{
			return Convert.ToDouble((int)ToConvert);
		}

		//*Should* be a supported type already
		return ToConvert;
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
		foreach(List<object> List in API.ExposeConstructors(API.LEVEL.SERVER_GM))
		{
			ServerGmEngine.SetGlobalValue((string)List[0], (ClrFunction)List[1]);
		}
	}


	public static void SetupClientEngine()
	{
		ClientGmEngine = new Jurassic.ScriptEngine();
		foreach(List<object> List in API.Expose(API.LEVEL.CLIENT_GM, Self))
		{
			ClientGmEngine.SetGlobalFunction((string)List[0], (Delegate)List[1]);
		}
		foreach(List<object> List in API.ExposeConstructors(API.LEVEL.CLIENT_GM))
		{
			ClientGmEngine.SetGlobalValue((string)List[0], (ClrFunction)List[1]);
		}
	}


	public override void _Ready()
	{
		File SetupScript = new File();
		SetupScript.Open("res://Scripting/SetupScript.js", 1);
		ConsoleEngine.Execute(SetupScript.GetAsText());
		SetupScript.Close();
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
		if(ModeDir.DirExists("user://gamemodes/" + Name)) //Gamemode exists
		{
			GamemodeName = Name;

			if(ModeDir.FileExists("user://gamemodes/" + Name + "/server.js")) //Has a server side script
			{
				SetupServerEngine();
				File ServerScript = new File();
				ServerScript.Open("user://gamemodes/" + Name + "/server.js", 1);
				ServerGmEngine.Execute(ServerScript.GetAsText());
				ServerScript.Close();
			}

			if(ModeDir.FileExists("user://gamemodes/" + Name + "/client.js")) //Has a client side script
			{
				SetupClientEngine();
				File ClientScriptFile = new File();
				ClientScriptFile.Open("user://gamemodes/" + Name + "/client.js", 1);
				ClientGmScript = ClientScriptFile.GetAsText();
				ClientScriptFile.Close();
				ClientGmEngine.Execute(ClientGmScript);
				Net.SteelRpc(Self, nameof(NetLoadClientScript), new object[] {ClientGmScript});
			}
		}
	}


	[Remote]
	public void NetLoadClientScript(string Script)
	{
		Console.Log("Recieved client.js from server, executing");
		SetupClientEngine();
		ClientGmEngine.Execute(Script);
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
