using Godot;
using System;

public class LoadHostButton : Button
{
	public string SaveName = "";


	public void LoadPressed()
	{
		Net.Host();
		World.Load(SaveName);
	}
}
