using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Tabby : Node
{
	PackedScene ScriptRootScene;
	Tabby()
	{
		ScriptRootScene = ResourceLoader.Load("res://TabbyScript/ScriptRoot.tscn") as PackedScene;
	}


	public enum OP {PUSH, STORE, GET, ADD, SUB, MULT, DIVI};
	public enum TYPE {BOOL, NUM, STR, NULL, VOID}


	public class DataClass
	{
		public TYPE Type;
		public object Data;

		public DataClass(TYPE TypeArg = Tabby.TYPE.VOID, object DataArg = null)
		{
			this.Type = TypeArg;
			this.Data = DataArg;
		}

		public override string ToString()
		{
			return Data.ToString();
		}

		public static DataClass operator+(DataClass A, DataClass B)
		{
			DataClass OutData = new DataClass();

			switch(A.Type)
			{
				case Tabby.TYPE.NUM:
					OutData.Type = Tabby.TYPE.NUM;
					OutData.Data = (int)A.Data + (int)B.Data;
					break;
			}

			return OutData;
		}
	}


	public void RunNewScript(string Script)
	{
		Node ScriptRootNode = ScriptRootScene.Instance();
		this.AddChild(ScriptRootNode);

		ScriptRoot Sroot = (ScriptRoot)ScriptRootNode;
		Sroot.ExecScript(Script);
		//Sroot.ExecScript("");
	}
}
