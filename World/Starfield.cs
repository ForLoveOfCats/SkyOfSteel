using Godot;
using System;
using static Godot.Mathf;



public class Starfield : Spatial
{
	private static PackedScene StarScene;

	static Starfield()
	{
		if(Engine.EditorHint) {return;}

		StarScene = GD.Load<PackedScene>("res://World/Star.tscn");
	}


	public override void _Ready()
	{
		Random Rand = new Random();
		for(int i = 0; i < 1000; i++)
		{
			MeshInstance Star = StarScene.Instance() as MeshInstance;
			AddChild(Star);

			Vector3 Pos = new Vector3(0, 0, 50000);
			Pos = Pos.Rotated(new Vector3(1,0,0), Deg2Rad(Rand.Next(-90, 0)));
			Pos = Pos.Rotated(new Vector3(0,1,0), Deg2Rad(Rand.Next(0, 360)));
			Star.Translation = Pos;
		}
	}


	public override void _Process(float Delta)
	{
		Camera Cam = GetViewport().GetCamera();
		if(Cam != null)
			Translation = Cam.GlobalTransform.origin;
	}
}
