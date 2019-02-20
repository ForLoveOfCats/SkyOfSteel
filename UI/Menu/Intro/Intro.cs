using Godot;
using System;

public class Intro : VBoxContainer
{
	public void ContinuePressed()
	{
		Menu.BuildNick();
	}
}
