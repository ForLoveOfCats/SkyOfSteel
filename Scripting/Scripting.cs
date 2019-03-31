using Godot;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sc = Microsoft.CodeAnalysis.Scripting;
using Cs = Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript;


public class Scripting : Node
{
	public static Sc.ScriptState ConsoleEngine;
	public static Sc.ScriptState GmEngine;

	public static string GamemodeName;

	public static Scripting Self;
	Scripting()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		Assembly[] LoadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

		Sc.Script CEngine = Cs.Create("", Sc.ScriptOptions.Default.WithReferences(LoadedAssemblies));
		ConsoleEngine = CEngine.ContinueWith("using System; using Godot; using static API;").RunAsync().Result;

		Sc.Script GEngine = Cs.Create("", Sc.ScriptOptions.Default.WithReferences(LoadedAssemblies));
		GmEngine = GEngine.ContinueWith("using System; using Godot; using static API;").RunAsync().Result;
	}


	public static void LoadGameMode(string Name)
	{
		UnloadGameMode();

		Directory ModeDir = new Directory();
		if(ModeDir.FileExists($"user://Gamemodes/{Name}/{Name}.csx")) //Has a  script
		{
			Console.Log($"Loaded gamemode '{Name}', executing");

			GamemodeName = Name;
			File ServerScript = new File();
			ServerScript.Open($"user://Gamemodes/{Name}/{Name}.csx", 1);

			try
			{
				Sc.ScriptState State = GmEngine.ContinueWithAsync(ServerScript.GetAsText()).Result;
				object Returned = State.ReturnValue;
				if(Returned is Gamemode)
				{
					Game.Mode = Returned as Gamemode;
					Game.Mode.LoadPath = $"{OS.GetUserDataDir()}/Gamemodes/{Name}";
					Game.Self.AddChild(Game.Mode);
					Game.Mode.SetName("Gamemode");
				}
				else
				{
					Console.ThrowLog($"Gamemode script '{Name}' did not return a valid Gamemode instance, unloading");
					UnloadGameMode();
				}
			}
			catch(Exception Err)
			{
				ServerScript.Close();
				Console.Log(Err.Message);
				UnloadGameMode();
			}

			ServerScript.Close();
		}
		else
		{
			Console.ThrowPrint($"No gamemode named '{Name}'");
		}
	}


	public static void UnloadGameMode()
	{
		if(GamemodeName != null)
		{
			Console.Log($"The gamemode '{GamemodeName}' was unloaded");
			GamemodeName = null;

			Game.Mode.OnUnload();
			Game.Mode.QueueFree(); //NOTE: Could cause issues with functions being called after OnUnload
			Game.Mode = new Gamemode();
		}
	}


	public static void RunConsoleLine(string Line)
	{
		try
		{
			ConsoleEngine = ConsoleEngine.ContinueWithAsync(Line).Result;
			object Returned = ConsoleEngine.ReturnValue;
			if(Returned != null)
			{
				Console.Print(Returned.ToString());
			}
		}
		catch(Exception Err)
		{
			Console.Print(Err.Message);
		}
	}
}
