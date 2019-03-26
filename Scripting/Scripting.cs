using Godot;
using System;
using System.Collections.Generic;


public class Scripting : Node
{
	// public static ScriptEngine ConsoleEngine;
	// public static ScriptEngine GmEngine;

	public static string GamemodeName;

	public static Scripting Self;
	Scripting()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		// ConsoleEngine = Python.CreateEngine(new Dictionary<string,object>() { {"DivisionOptions", PythonDivisionOptions.New} });

		SetupGmEngine();

		File SetupScript = new File();
		SetupScript.Open("res://Scripting/SetupScript.py", 1);
		try
		{
			// ScriptSource Source = ConsoleEngine.CreateScriptSourceFromString(SetupScript.GetAsText(), SourceCodeKind.Statements);
			// Source.Execute(ConsoleScope);
		}
		catch(Exception Err)
		{
			SetupScript.Close();
			// ExceptionOperations EO = ConsoleEngine.GetService<ExceptionOperations>();
			// GD.Print(EO.FormatException(Err));
			throw new Exception($"Encountered error running SetupScript.py check editor Output pane or stdout");
		}
		SetupScript.Close();
	}


	public static void SetupGmEngine()
	{
		// GmEngine = Python.CreateEngine(new Dictionary<string,object>() { {"DivisionOptions", PythonDivisionOptions.New} });
	}


	public override void _PhysicsProcess(float Delta)
	{
		object Function = null;
		/*ConsoleScope.TryGetVariable("_tick", out Function);
		if(Function != null && Function is PythonFunction)
		{
		try
		{
		ConsoleEngine.Operations.Invoke(Function, Delta);
		}
		catch(Exception Err)
		{
		//TODO figure out a better solution to this
		//Currently we just dump an error every _tick call
		//Eventually I need to mark if the _tick functin has an error and not run it
		//However that would fall apart once one can define functions at runtime
		ExceptionOperations EO = Scripting.ConsoleEngine.GetService<ExceptionOperations>();
		Console.Print(EO.FormatException(Err));
		}
		}*/

		if(GamemodeName != null)
		{
			Function = null;
			/*GmScope.TryGetVariable("_tick", out Function);
			  if(Function != null && Function is PythonFunction)
			  {
			  try
			  {
			  GmEngine.Operations.Invoke(Function, Delta);
			  }
			  catch(Exception Err)
			  {
			  ExceptionOperations EO = Scripting.GmEngine.GetService<ExceptionOperations>();
			  Console.Print(EO.FormatException(Err));
			  }
			  }*/
		}
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
		/*try
		  {
		  object Returned = ConsoleEngine.Execute(Line, ConsoleScope);
		  if(Returned != null)
		  {
		  Console.Print(PyToString(Returned));
		  }
		  }
		  catch(Exception e)
		  {
		  ExceptionOperations eo = ConsoleEngine.GetService<ExceptionOperations>();
		  Console.Print(eo.FormatException(e));
		  }*/
	}
}
