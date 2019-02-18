using Godot;
using System;

public class LoadSaveHostButton : Button
{
	public string SaveName = "";


	public void LoadPressed()
	{
		Net.Host();
		Game.LoadWorld(SaveName);
	}
}
