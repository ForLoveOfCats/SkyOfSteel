using Godot;
using System;

public class LoadHostButton : Button
{
	public string SaveName = "";
	public AudioStreamPlayer MouseOverSfx;

	public override void _Ready()
	{
		MouseOverSfx = GetNode<AudioStreamPlayer>("MouseOverSfx");
	}

	public void LoadPressed()
	{
		Net.Host();
		World.Load(SaveName);
	}


	public void MouseEnter()
	{
		MouseOverSfx.Play();
	}
}
