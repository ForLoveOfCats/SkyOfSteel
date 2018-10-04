using Godot;
using System;
using Jurassic;

public class ScriptEngine : Node
{
	public override void _Ready()
	{
		var engine = new Jurassic.ScriptEngine();
		Console.WriteLine(engine.Evaluate("5 * 10 + 2"));
	}
}
