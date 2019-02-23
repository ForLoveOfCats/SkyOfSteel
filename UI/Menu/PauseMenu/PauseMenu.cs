using Godot;


public class PauseMenu : VBoxContainer
{
	public override void _Ready()
	{
		GetNode<Label>("Version").Text = $"Version: {Game.Version}";
	}


	public void ReturnPressed()
	{
		Menu.Close();
	}


	public void SavePressed()
	{
		Menu.BuildSave();
	}


	public void DisconnectPressed()
	{
		Net.Disconnect();
	}


	public void QuitPressed()
	{
		Game.Quit();
	}
}
