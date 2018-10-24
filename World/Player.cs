using Godot;
using System;


public class Player : Spatial
{
	public bool Possessed = false;


	public override void _Ready()
	{
		Translation = new Vector3(0,1,0);
		if(Possessed)
		{
			((Camera)GetNode("SteelCamera")).MakeCurrent();
			((MeshInstance)GetNode("FPSMesh")).Hide();
			AddChild(((PackedScene)GD.Load("res://UI/SteelHUD.tscn")).Instance());
		}
	}
}
