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
			case "host":
				return new List<object> {"host", new Action(delegate(){
					((SceneTree)Engine.GetMainLoop()).GetRoot().GetNode("/root/Net").Call("host", new string[] {"7777"});
				})};
			case "connect":
				return new List<object> {"connect", new Action<string>(delegate(string Ip){
					if(Ip == "" || Ip == "localhost")
					{
						Ip = "127.0.0.1";
					}
					((SceneTree)Engine.GetMainLoop()).GetRoot().GetNode("/root/Net").Call("connect", new string[] {Ip, "7777"});
				})};
			case "get_ms":
				return new List<object> {"get_ms", new Func<int>(() => {return OS.GetTicksMsec();})};
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
				Output.Add(GetDelCall("get_ms", ScriptingRef));
				Output.Add(GetDelCall("host", ScriptingRef));
				Output.Add(GetDelCall("connect", ScriptingRef));
				break;
			case LEVEL.SERVER_GM:
				Output.Add(GetDelCall("log", ScriptingRef));
				Output.Add(GetDelCall("get_ms", ScriptingRef));
				break;
			case LEVEL.CLIENT_GM:
				Output.Add(GetDelCall("log", ScriptingRef));
				Output.Add(GetDelCall("get_ms", ScriptingRef));
				break;
		}

		return Output;
	}
}
