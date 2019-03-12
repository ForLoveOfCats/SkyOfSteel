using Godot;
using System;
using System.Collections.Generic;
using IronPython;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;


public class Scripting : Node
{
	public static ScriptEngine ConsoleEngine;
	public static ScriptScope ConsoleScope;
	public static ScriptScope StringScope; //Used to convert objects to string
	public static ScriptEngine GmEngine;
	public static ScriptScope GmScope;

	public static string GamemodeName;

	public static Scripting Self;
	Scripting()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		ConsoleEngine = Python.CreateEngine(new Dictionary<string,object>() { {"DivisionOptions", PythonDivisionOptions.New} });
		ConsoleScope = ConsoleEngine.CreateScope();
		StringScope = ConsoleEngine.CreateScope();
        

		foreach(List<object> List in API.Expose(API.LEVEL.CONSOLE, this))
		{
			ConsoleScope.SetVariable((string)List[0], (Delegate)List[1]);
		}
		foreach(API.PyConstructorExposer Exposer in API.ExposeConstructors(API.LEVEL.CONSOLE))
		{
			ConsoleScope.SetVariable(Exposer.Name, Exposer.Constructor);
		}
        

		SetupGmEngine();

		File SetupScript = new File();
		SetupScript.Open("res://Scripting/SetupScript.py", 1);
		try
		{
			ScriptSource Source = ConsoleEngine.CreateScriptSourceFromString(SetupScript.GetAsText(), SourceCodeKind.Statements);
			Source.Execute(ConsoleScope);
		}
		catch(Exception Err)
		{
			SetupScript.Close();
			ExceptionOperations EO = ConsoleEngine.GetService<ExceptionOperations>();
			GD.Print(EO.FormatException(Err));
			throw new Exception($"Encountered error running SetupScript.py check editor Output pane or stdout");
		}
		SetupScript.Close();
	}


	public static object ToPy(object ToConvert)
	{
		if(ToConvert is Vector3)
		{
			//Could use two layers of casting to implicitly convert
			//This is just nicer to read
			return new PyVector3((Vector3)ToConvert);
		}

		//Does not require intervention
		return ToConvert;
	}


	public static object[] ToPy(params object[] ConvertArray)
	{
		object[] Out = new object[ConvertArray.Length];
		int Iteration = 0;
		foreach(object ToConvert in ConvertArray)
		{
			Out[Iteration] = ToPy(ToConvert);
			Iteration += 1;
		}
		return Out;
	}

	public static void SetupGmEngine()
	{
		GmEngine = Python.CreateEngine(new Dictionary<string,object>() { {"DivisionOptions", PythonDivisionOptions.New} });
		GmScope = ConsoleEngine.CreateScope();

		foreach(List<object> List in API.Expose(API.LEVEL.GAMEMODE, Self))
		{
			GmScope.SetVariable((string)List[0], (Delegate)List[1]);
		}
		foreach(API.PyConstructorExposer Exposer in API.ExposeConstructors(API.LEVEL.CONSOLE))
		{
			GmScope.SetVariable(Exposer.Name, Exposer.Constructor);
		}
	}


	public override void _PhysicsProcess(float Delta)
	{
		object Function = null;
		ConsoleScope.TryGetVariable("_tick", out Function);
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
		}

		if(GamemodeName != null)
		{
			Function = null;
			GmScope.TryGetVariable("_tick", out Function);
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
			}
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
				GmEngine.Execute(ServerScript.GetAsText(), GmScope);
			}
			catch(Exception Err)
			{
				ExceptionOperations EO = GmEngine.GetService<ExceptionOperations>();
				Console.Print(EO.FormatException(Err));
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


	public static string PyToString(object Obj)
	{
		StringScope.SetVariable("to_string_object", Obj);
		return ConsoleEngine.Execute("str(to_string_object)", StringScope);
	}


	public static void RunConsoleLine(string Line)
	{
		try
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
		}
	}
}
