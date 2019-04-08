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

	public static string GamemodeName;

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
		ConsoleState = CEngine.ContinueWith("using System; using System.Dynamic; using Godot; using static API;").RunAsync().Result;
	}


	[Remote]
	public void RequestGmLoad(string Name)
	{
		Console.Log($"The server requested that gamemode '{Name}' be loaded");
		if(LoadGameMode(Name))
		{
			Console.Log($"Successfully loaded the gamemode '{Name}' as requested by the server");
		}
		else
		{
			Console.ThrowLog($"The server requested that your client load the gamemode '{Name}' but your client was unable to");
		}
	}


	public static bool LoadGameMode(string Name)
	{
		UnloadGameMode();

		Directory ModeDir = new Directory();
		if(ModeDir.FileExists($"user://Gamemodes/{Name}/{Name}.json"))
		{
			Console.Log($"Found gamemode '{Name}', loading");

			GmConfigClass Config;
			{
				File ConfigFile = new File();
				ConfigFile.Open($"user://Gamemodes/{Name}/{Name}.json", 1);
				try
				{
					Config = Newtonsoft.Json.JsonConvert.DeserializeObject<GmConfigClass>(ConfigFile.GetAsText());
					ConfigFile.Close();
				}
				catch(Newtonsoft.Json.JsonReaderException)
				{
					ConfigFile.Close();
					Console.ThrowLog($"Failed to parse config file for gamemode '{Name}'");
					return false;
				}
			}
			if(Config.MainScript == null)
			{
				Console.ThrowLog($"The gamemode '{Name}' did not specify a path for MainScript");
				return false;
			}

			File ScriptFile = new File();
			if(!ScriptFile.FileExists($"user://Gamemodes/{Name}/{Config.MainScript}"))
			{
				Console.ThrowLog($"Specified MainScript '{Config.MainScript}' for gamemode '{Name}' does not exist");
				return false;
			}
			ScriptFile.Open($"user://Gamemodes/{Name}/{Config.MainScript}", 1);
			Sc.Script Engine = Cs.Create(ScriptFile.GetAsText(),
			                             ScriptOptions.WithSourceResolver(new Microsoft.CodeAnalysis.SourceFileResolver(ImmutableArray<string>.Empty, $"{OS.GetUserDataDir()}/Gamemodes/{Name}")));
			ScriptFile.Close();

			object Returned = null;
			try
			{
				Sc.ScriptState State = Engine.RunAsync().Result;
				Returned = State.ReturnValue;
			}
			catch(Exception Err)
			{
				Console.ThrowLog($"Error executing gamemode '{Name}': {Err.Message}");
				return false;
			}

			if(Returned is Gamemode)
			{
				GamemodeName = Name;
				Game.Mode = Returned as Gamemode;
				Game.Mode.LoadPath = $"{OS.GetUserDataDir()}/Gamemodes/{Name}";
				Game.Mode.OwnName = Name;
				Game.Self.AddChild(Game.Mode);
				Game.Mode.SetName("Gamemode");
				return true;
			}
			else
			{
				Console.ThrowLog($"Gamemode script '{Name}' did not return a valid Gamemode instance, unloading");
				return false;
			}
		}
		else
		{
			Console.ThrowPrint($"No gamemode named '{Name}'");
			return false;
		}
	}


	[Remote]
	public void RequestGmUnload()
	{
		UnloadGameMode();
	}


	public static void UnloadGameMode()
	{
		if(GamemodeName != null)
		{
			try
			{
				Game.Mode.OnUnload();
			}
			catch(Exception Err)
			{
				Console.ThrowLog($"An exception was thrown when calling 'OnUnload' on gamemode '{GamemodeName}': {Err.Message}");
			}

			Game.Mode.QueueFree(); //NOTE: Could cause issues with functions being called after OnUnload
			Game.Mode = new Gamemode();
			API.Gm = new API.EmptyCustomCommands();

			Console.Log($"The gamemode '{GamemodeName}' was unloaded");
			GamemodeName = null;
		}
	}


	public static void RunConsoleLine(string Line)
	{
		object Returned = null;

		try
		{
			ConsoleState = ConsoleState.ContinueWithAsync(Line).Result;
			Returned = ConsoleState.ReturnValue as object;
		}
		catch(Sc.CompilationErrorException Err)
		{
			Console.Print(Err.Message);
		}

		if(Returned != null)
		{
			Console.Print(Returned);
		}
	}
}
