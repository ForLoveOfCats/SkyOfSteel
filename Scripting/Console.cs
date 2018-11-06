using Godot;
using System;
using System.Collections.Generic;


public class Console : Node
{
	private static Node Window;
	private static List<string> History = new List<string>();
	private static int HistLocal = 0;


	private static Console Self;
	private Console()
	{
		Self = this;
	}


	public override void _Ready()
	{
		Window = GetTree().GetRoot().GetNode("SteelGame/ConsoleWindow");
		Console.Print("");
		Console.Log("");
	}


	public override void _Process(float delta)
	{
		if(Input.IsActionJustPressed("ui_up") && HistLocal > 0)
		{
			HistLocal -= 1;
			((LineEdit)Window.GetNode("LineEdit")).Text = History[HistLocal];
		}

		if(Input.IsActionJustPressed("ui_down") && HistLocal < History.Count)
		{
			HistLocal += 1;
			if(HistLocal == History.Count)
			{
				((LineEdit)Window.GetNode("LineEdit")).Text = "";
			}
			else
			{
				((LineEdit)Window.GetNode("LineEdit")).Text = History[HistLocal];
			}
		}
	}


	public static void Print(string ToPrint)
	{
		((TextEdit)Window.GetNode("HBox/Console")).Text += ToPrint + "\n";
	}


	public static void Log(string ToLog)
	{
		((TextEdit)Window.GetNode("HBox/Log")).Text += ToLog + "\n";
	}


	public static void Execute(string Command)
	{
		Console.Print("\n>>> " + Command);
		History.Add(Command);
		HistLocal = History.Count;
		Scripting.RunConsoleLine(Command);
	}
}
