using Godot;


public class ConsoleWindow : Panel {
	bool IsOpen;
	LineEdit InputLine;


	public void Open() {
		Show();
		IsOpen = true;
		InputLine.Editable = true;
		InputLine.Text = "";
		InputLine.GrabFocus();
	}


	public void Close() {
		Hide();
		IsOpen = false;
		InputLine.Editable = false;
		InputLine.Text = "";
	}


	public override void _Ready() {
		InputLine = GetNode("VBox/LineEdit") as LineEdit;
		Close(); //Console.IsOpen should already be false
	}


	public override void _Process(float Delta) {
		if(Input.IsActionJustPressed("Enter") && IsOpen) {
			Console.Execute(InputLine.Text);
			InputLine.Text = "";
		}
	}
}
