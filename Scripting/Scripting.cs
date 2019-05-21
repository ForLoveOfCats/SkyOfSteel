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

		#if !TOOLS //We need to extract all assemblies into the filesystem for Roslyn to use
		System.IO.Directory.CreateDirectory($"{System.IO.Directory.GetCurrentDirectory()}/.mono/assemblies");
		Directory Dir = new Directory();
		Dir.Open("res://.mono/assemblies/");
		Dir.ListDirBegin(skipNavigational:true, skipHidden:true);
		string Name = Dir.GetNext();
		while(Name != "")
		{
			if(!Dir.FileExists($"{System.IO.Directory.GetCurrentDirectory()}/.mono/assemblies/{Name}"))
				Dir.Copy($"res://.mono/assemblies/{Name}", $"{System.IO.Directory.GetCurrentDirectory()}/.mono/assemblies/{Name}");
			Name = Dir.GetNext();
		}
		Dir.ListDirEnd();
		#endif

		ScriptOptions = Sc.ScriptOptions.Default.WithReferences(AppDomain.CurrentDomain.GetAssemblies())
			.AddReferences(Assembly.GetAssembly(typeof(System.Dynamic.DynamicObject)),  // System.Code
						   Assembly.GetAssembly(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo)),  // Microsoft.CSharp
						   Assembly.GetAssembly(typeof(System.Dynamic.ExpandoObject)));  // System.Dynamic

		Sc.Script CEngine = Cs.Create("", ScriptOptions);
		ConsoleState = CEngine.ContinueWith("using System; using System.Dynamic; using Godot; using static API; using static Items.TYPE;")
			.RunAsync().Result;
	}


	[Remote]
	public void RequestGmLoad(string Name)
	{
		Console.Log($"The server requested that gamemode '{Name}' be loaded");
		if(LoadGamemode(Name))
		{
			Console.Log($"Successfully loaded the gamemode '{Name}' as requested by the server");
		}
		else
		{
			Console.ThrowLog($"The server requested that your client load the gamemode '{Name}' but your client was unable to");
		}
	}


	public static bool LoadGamemode(string Name)
	{
		UnloadGamemode();

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
			                             ScriptOptions.WithSourceResolver(new Microsoft.CodeAnalysis.SourceFileResolver(ImmutableArray<string>.Empty, $"{OS.GetUserDataDir()}/Gamemodes/{Name}"))
			                             .WithEmitDebugInformation(true)
			                             .WithFilePath($"{OS.GetUserDataDir()}/Gamemodes/{Name}")
			                             .WithFileEncoding(System.Text.Encoding.UTF8)); //NOTE Hardcoding UTF8 should work for now
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
				Game.Mode.SetName("Gamemode");
				Game.Mode.Self = Game.Mode;
				Game.Mode.LoadPath = $"{OS.GetUserDataDir()}/Gamemodes/{Name}";
				Game.Mode.OwnName = Name;
				Game.Self.AddChild(Game.Mode);
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
		UnloadGamemode();
	}


	public static void UnloadGamemode()
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

			Game.Mode.SetName("UnloadedGamemode"); ///Prevents name mangling of new gamemode, important for RPC
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
			Returned = ConsoleState.ReturnValue as object; //just in case of an issue this should become null
		}
		catch(Exception Err)
		{
			Console.Print(Err.Message);
		}

		if(Returned != null)
		{
			Console.Print(Returned);
		}
	}
}
