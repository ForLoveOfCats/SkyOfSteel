using Godot;


public class PauseMenu : VBoxContainer
{
	public override void _Ready()
	{
		GetNode<Label>("Version").Text = $"Version: {Game.Version}";

		Label PlayingOn = GetNode<Label>("PlayingOn");
		if(Net.Work.IsNetworkServer())
		{
			PlayingOn.Text = $"Hosting map: {World.SaveName}";
		}
		else
		{
			PlayingOn.Text = $"Connected to server at: {Net.Ip}";
		}
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
