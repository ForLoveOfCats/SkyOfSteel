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
	// public static ScriptEngine GmEngine;

	public static string GamemodeName;

	public static Scripting Self;
	Scripting()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		// ConsoleEngine = Python.CreateEngine(new Dictionary<string,object>() { {"DivisionOptions", PythonDivisionOptions.New} });
		Sc.Script CEngine = Cs.Create(@"", Sc.ScriptOptions.Default.WithReferences(AppDomain.CurrentDomain.GetAssemblies()));
		ConsoleEngine = CEngine.ContinueWith("using static API; using Godot;").RunAsync().Result;

		SetupGmEngine();
	}


	public static void SetupGmEngine()
	{
		// GmEngine = Python.CreateEngine(new Dictionary<string,object>() { {"DivisionOptions", PythonDivisionOptions.New} });
	}


	public override void _PhysicsProcess(float Delta)
	{
		//TODO Call the gamemode's tick function
	}


	public static void LoadGameMode(string Name)
	{
		UnloadGameMode();

		Directory ModeDir = new Directory();
		if(ModeDir.FileExists($"user://gamemodes/{Name}/{Name}.py")) //Has a  script
		{
			Console.Log($"Loaded gamemode '{Name}', executing");

			GamemodeName = Name;
			SetupGmEngine();
			File ServerScript = new File();
			ServerScript.Open($"user://gamemodes/{Name}/{Name}.py", 1);

			try
			{
				// GmEngine.Execute(ServerScript.GetAsText(), GmScope);
			}
			catch(Exception Err)
			{
				// ExceptionOperations EO = GmEngine.GetService<ExceptionOperations>();
				// Console.Print(EO.FormatException(Err));
				Scripting.UnloadGameMode();
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
			SetupGmEngine();
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
