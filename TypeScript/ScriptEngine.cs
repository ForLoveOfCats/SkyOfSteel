using Godot;
using System;
using Jurassic;

public class ScriptEngine : Node
{
	public void RunNewScript(string Script)
	{
		Jurassic.ScriptEngine Engine = new Jurassic.ScriptEngine();
		GD.Print(Engine.Evaluate(Script));
	}
}
