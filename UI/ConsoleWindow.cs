using Godot;


class ConsoleWindow : VBoxContainer
{
	bool IsWindowVisible;
	LineEdit InputLine;


	public void WindowVisible(bool ShouldBeVisible)
	{
		if(ShouldBeVisible)
		{
			Show();
			IsWindowVisible = true;
			InputLine.SetEditable(true);
			InputLine.Text = "";
			InputLine.GrabFocus();
		}
		else
		{
			Hide();
			IsWindowVisible = true;
			InputLine.SetEditable(false);
			InputLine.Text = "";
		}
	}


	public override void _Ready()
	{
		InputLine = GetNode("LineEdit") as LineEdit;
		WindowVisible(true);
	}


	public override void _Process(float Delta)
	{
		if(Input.IsActionJustPressed("Enter") && IsWindowVisible)
		{
			Console.Execute(InputLine.Text);
			InputLine.Text = "";
		}
	}
}
