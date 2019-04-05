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

		Sc.ScriptOptions Options = Sc.ScriptOptions.Default.WithReferences(AppDomain.CurrentDomain.GetAssemblies())
			.AddReferences(Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject)),  // System.Code
						   Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),  // Microsoft.CSharp
						   Assembly.GetAssembly(typeof(System.Dynamic.ExpandoObject)));  // System.Dynamic

		Sc.Script CEngine = Cs.Create("", Options);
		ConsoleEngine = CEngine.ContinueWith("using System; using System.Dynamic; using Godot; using static API;").RunAsync().Result;

		Sc.Script GEngine = Cs.Create("", Options);
		GmEngine = GEngine.ContinueWith("using System; using System.Dynamic; using Godot; using static API;").RunAsync().Result;
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

			string Source = "";
			{
				File ScriptFile = new File();
				foreach(string Path in Config.Scripts)
				{
					if(!ModeDir.FileExists($"user://Gamemodes/{Name}/{Path}"))
					{
						Console.ThrowLog($"No file '{Path}' while loading scripts for gamemode '{Name}'");
						ScriptFile.Close();
						return false;
					}

					ScriptFile.Open($"user://Gamemodes/{Name}/{Path}", 1);
					Source += "\n" + ScriptFile.GetAsText();
				}

				ScriptFile.Close();
			}

			try
			{
				Sc.ScriptState State = GmEngine.ContinueWithAsync(Source).Result;
				object Returned = State.ReturnValue;
				if(Returned is Gamemode)
				{
					GamemodeName = Name;
					Game.Mode = Returned as Gamemode;
					Game.Mode.LoadPath = $"{OS.GetUserDataDir()}/Gamemodes/{Name}";
					Game.Mode.Name = Name;
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
			catch(Exception Err)
			{
				Console.Log(Err.Message);
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
		try
		{
			ConsoleEngine = ConsoleEngine.ContinueWithAsync(Line).Result;
			object Returned = ConsoleEngine.ReturnValue as object;
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
