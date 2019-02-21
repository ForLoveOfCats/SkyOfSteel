using Godot;


class ConsoleWindow : VBoxContainer
{
	bool IsOpen;
	LineEdit InputLine;


	public void Open()
	{
		Show();
		IsOpen = true;
		InputLine.SetEditable(true);
		InputLine.Text = "";
		InputLine.GrabFocus();
	}


	public void Close()
	{
		Hide();
		IsOpen = false;
		InputLine.SetEditable(false);
		InputLine.Text = "";
	}


	public override void _Ready()
	{
		InputLine = GetNode("LineEdit") as LineEdit;
		Close(); //Console.IsOpen should already be false
	}


	public override void _Process(float Delta)
	{
		if(Input.IsActionJustPressed("Enter") && IsOpen)
		{
			Console.Execute(InputLine.Text);
			InputLine.Text = "";
		}
	}
}
