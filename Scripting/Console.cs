using Godot;
using System;
using System.Collections.Generic;


public class Console : Node
{
	public static bool IsOpen = false;

	public static ConsoleWindow Window;
	public static LineEdit InputLine;
	public static RichTextLabel ConsoleLabel;
	public static RichTextLabel LogLabel;
	public static List<string> History = new List<string>();
	public static int HistoryLocation = 0;


	public static Console Self;
	private Console()
	{
		Self = this;
	}


	public override void _Ready()
	{
		Window = GetTree().Root.GetNode("RuntimeRoot/ConsoleWindow") as ConsoleWindow;
		InputLine = Window.GetNode("VBox/LineEdit") as LineEdit;
		ConsoleLabel = Window.GetNode("VBox/HBox/Console") as RichTextLabel;
		LogLabel = Window.GetNode("VBox/HBox/Log") as RichTextLabel;
		Console.Print("");
		LogLabel.Text += "\n";
	}


	public override void _Input(InputEvent Event)
	{
		if(Event.IsAction("ui_up"))
		{
			GetTree().SetInputAsHandled();
			InputLine.GrabFocus();

			if(Input.IsActionJustPressed("ui_up") && HistoryLocation > 0)
			{
				HistoryLocation -= 1;
				InputLine.Text = History[HistoryLocation];

				InputLine.CaretPosition = InputLine.Text.Length;
			}
		}

		if(Event.IsAction("ui_down"))
		{
			GetTree().SetInputAsHandled();
			InputLine.GrabFocus();

			if(Input.IsActionJustPressed("ui_down") && HistoryLocation < History.Count)
			{
				HistoryLocation += 1;
				if(HistoryLocation == History.Count)
				{
					InputLine.Text = "";
				}
				else
				{
					InputLine.Text = History[HistoryLocation];
				}

				InputLine.CaretPosition = InputLine.Text.Length;
			}
		}
	}


	public static void Print(object ToPrint)
	{
		ConsoleLabel.Text += $"{ToPrint}\n";
	}


	public static void Log(object ToLog)
	{
		LogLabel.Text += $"{ToLog}\n\n";
	}


	public static void ThrowPrint(object ToThrow)
	{
		Print($"ERROR: {ToThrow}");
	}


	public static void ThrowLog(object ToThrow)
	{
		Log($"ERROR: {ToThrow}");
	}


	public static void Execute(string Command)
	{
		Console.Print("\n>>> " + Command);

		if(History.Count <= 0 || History[History.Count-1] != Command)
		{
			History.Add(Command);
		}
		HistoryLocation = History.Count;

		Scripting.RunConsoleLine(Command);
	}


	public static void Close()
	{
		Window.Close();
		IsOpen = false;
		HistoryLocation = History.Count;

		if(!Menu.IsOpen)
		{
			Input.SetMouseMode(Input.MouseMode.Captured);
			Game.BindsEnabled = true;
		}
	}


	public static void Open()
	{
		Window.Open();
		IsOpen = true;
		HistoryLocation = History.Count;

		Input.SetMouseMode(Input.MouseMode.Visible);
		Game.BindsEnabled = false;
	}
}
