using Godot;
using System;

public class HostMenu : VBoxContainer
{
	public void HostPressed()
	{
		Net.Host();
	}


	public void BackPressed()
	{
		Menu.BuildMain();
	}
}
