using Godot;
using System;

public class Intro : VBoxContainer
{
	public void ContinuePressed()
	{
		if(Game.RemoteVersion != null && Game.Version != Game.RemoteVersion)
			Menu.BuildUpdate();
		else
			Menu.BuildNick();
	}


	public void QuitPressed()
	{
		Game.Quit();
	}
}
