using Godot;
using System;
using System.Collections.Generic;
using Jurassic;

public delegate void DelVoidPassString(string In);

public class API : Node
{
	public enum LEVEL {ADMIN, SERVER_GM, CLIENT_GM};


	static List<object> GetDelCall(string Name, Scripting ScriptingRef)
	{
		switch(Name)
		{
			case "print":
				return new List<object> {"print", new DelVoidPassString(ScriptingRef.ApiPrint)};
			case "log":
				return new List<object> {"log", new DelVoidPassString(ScriptingRef.ApiLog)};
		}
	return new List<object>();
	}


	public static List<List<object>> Expose(LEVEL ApiLevel, Scripting ScriptingRef)
	{
		List<List<object>> Output = new List<List<object>>();

		switch(ApiLevel)
		{
			case LEVEL.ADMIN:
				Output.Add(GetDelCall("print", ScriptingRef));
				Output.Add(GetDelCall("log", ScriptingRef));
				break;
			case LEVEL.SERVER_GM:
				Output.Add(GetDelCall("log", ScriptingRef));
				break;
			case LEVEL.CLIENT_GM:
				Output.Add(GetDelCall("log", ScriptingRef));
				break;
		}

		return Output;
	}
}
