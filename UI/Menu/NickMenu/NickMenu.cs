using Godot;


public class NickMenu : VBoxContainer
{
	private LineEdit NameEdit;
	private Label AlertLabel;

	public override void _Ready()
	{
		NameEdit = GetNode<LineEdit>("HBoxContainer/NameEdit");
		AlertLabel = GetNode<Label>("AlertLabel");

		NameEdit.GrabFocus();
	}


	public override void _Process(float Delta)
	{
		if(Game.Nickname != Game.DefaultNickname)
		{
			Menu.BuildMain();
		}
	}


	public void EnterPressed(string Text)
	{
		ConfirmPressed();
	}


	public void ConfirmPressed()
	{
		if(NameEdit.Text == "")
		{
			AlertLabel.Text = "Please input your prefered multiplayer nickname to continue";
		}
		else
		{
			Game.Nickname = NameEdit.Text;
			Menu.BuildMain();
		}
	}
}
