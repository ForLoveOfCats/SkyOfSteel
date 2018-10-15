using Godot;
using System;
using System.Collections.Generic;
using Jurassic;


public class API : Node
{
	public enum LEVEL {ADMIN, SERVER_GM, CLIENT_GM};


	static List<object> GetDelCall(string Name)
	{
		switch(Name)
		{
			case "print":
				return new List<object> {"print", new Action<string>(delegate(string ToPrint){Console.Print(ToPrint);})};
			case "log":
				return new List<object> {"log", new Action<string>(delegate(string ToLog){Console.Log(ToLog);})};
			case "host":
				return new List<object> {"host", new Action(delegate(){
					Net.Host();
				})};
			case "connect":
				return new List<object> {"connect", new Action<string>(delegate(string Ip){
					if(Ip == "" || Ip == "localhost" || Ip == "undefined")
					{
						Ip = "127.0.0.1";
					}
					Net.ConnectTo(Ip);
				})};
			case "get_ms":
				return new List<object> {"get_ms", new Func<int>(() => {return OS.GetTicksMsec();})};
			case "get_peerlist":
				return new List<object> {"get_peerlist", new Func<Jurassic.Library.ArrayInstance>(() => {
					Jurassic.Library.ArrayInstance Out = Scripting.ConsoleEngine.Array.Construct();
					foreach(int Id in Net.PeerList)
					{
						Out.Push(Id);
					}
					return Out;
				})};
		}
	return new List<object>();
	}


	public static List<List<object>> Expose(LEVEL ApiLevel, Scripting ScriptingRef)
	{
		List<List<object>> Output = new List<List<object>>();

		switch(ApiLevel)
		{
			case LEVEL.ADMIN:
				Output.Add(GetDelCall("print"));
				Output.Add(GetDelCall("log"));
				Output.Add(GetDelCall("get_ms"));
				Output.Add(GetDelCall("host"));
				Output.Add(GetDelCall("connect"));
				Output.Add(GetDelCall("get_peerlist"));
				break;
			case LEVEL.SERVER_GM:
				Output.Add(GetDelCall("log"));
				Output.Add(GetDelCall("get_ms"));
				Output.Add(GetDelCall("get_peerlist"));
				break;
			case LEVEL.CLIENT_GM:
				Output.Add(GetDelCall("log"));
				Output.Add(GetDelCall("get_ms"));
				Output.Add(GetDelCall("get_peerlist"));
				break;
		}

		return Output;
	}
}
