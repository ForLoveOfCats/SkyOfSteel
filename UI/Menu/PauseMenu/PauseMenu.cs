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

		if(!Net.Work.IsNetworkServer())
		{
			Button SaveButton = GetNode<Button>("SaveButton");
			SaveButton.Disabled = true;
			SaveButton.HintTooltip = "Cannot save as client";
			SaveButton.MouseDefaultCursorShape = CursorShape.Arrow;
		}
	}


	public void ReturnPressed()
	{
		Menu.Close();
	}


	public void SavePressed()
	{
		if(Net.Work.IsNetworkServer())
			World.Save(World.SaveName);
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
