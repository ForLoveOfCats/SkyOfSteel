using Godot;
using System;

public class PauseMenu : VBoxContainer
{
	public void DisconnectPressed()
	{
		Net.Disconnect();
	}


	public void QuitPressed()
	{
		Game.Quit();
	}
}
