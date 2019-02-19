using Godot;
using System;

public class HostMenu : VBoxContainer
{
	public void HostBlankPressed()
	{
		Net.Host();
	}


	public void HostSavedPressed()
	{
		Menu.BuildLoadHost();
	}


	public void BackPressed()
	{
		Menu.BuildMain();
	}
}
