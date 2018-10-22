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
				return new List<object> {Name, new Action<string>(delegate(string ToPrint){Console.Print(ToPrint);})};

			case "log":
				return new List<object> {Name, new Action<string>(delegate(string ToLog){Console.Log(ToLog);})};

			case "host":
				return new List<object> {Name, new Action(delegate(){
					Net.Host();
				})};

			case "connect":
				return new List<object> {Name, new Action<string>(delegate(string Ip){
					if(Ip == "" || Ip == "localhost" || Ip == "undefined")
					{
						Ip = "127.0.0.1";
					}
					Net.ConnectTo(Ip);
				})};

			case "ms_get":
				return new List<object> {Name, new Func<int>(() => {return OS.GetTicksMsec();})};

			case "peerlist_get":
				return new List<object> {Name, new Func<Jurassic.Library.ArrayInstance>(() => {
					Jurassic.Library.ArrayInstance Out = Scripting.ConsoleEngine.Array.Construct();
					foreach(int Id in Net.PeerList)
					{
						Out.Push(Id);
					}
					return Out;
				})};

			case "bind":
				return new List<object> {Name, new Action<string, string>(delegate(string FunctionName, string InputString){
					Bindings.Bind(FunctionName, InputString);
				})};

			case "unbind":
				return new List<object> {Name, new Action<string>(delegate(string FunctionName){
					Bindings.UnBind(FunctionName);
				})};

			default:
				throw new System.ArgumentException("Invalid GetDelCall name arg '" + Name + "'");
		}
	}


	public static List<List<object>> Expose(LEVEL ApiLevel, Scripting ScriptingRef)
	{
		List<List<object>> Output = new List<List<object>>();

		switch(ApiLevel)
		{
			case LEVEL.ADMIN:
				Output.Add(GetDelCall("print"));
				Output.Add(GetDelCall("log"));
				Output.Add(GetDelCall("ms_get"));
				Output.Add(GetDelCall("host"));
				Output.Add(GetDelCall("connect"));
				Output.Add(GetDelCall("peerlist_get"));
				Output.Add(GetDelCall("bind"));
				Output.Add(GetDelCall("unbind"));
				break;
			case LEVEL.SERVER_GM:
				Output.Add(GetDelCall("log"));
				Output.Add(GetDelCall("ms_get"));
				Output.Add(GetDelCall("peerlist_get"));
				break;
			case LEVEL.CLIENT_GM:
				Output.Add(GetDelCall("log"));
				Output.Add(GetDelCall("ms_get"));
				Output.Add(GetDelCall("peerlist_get"));
				break;
		}

		return Output;
	}
}
