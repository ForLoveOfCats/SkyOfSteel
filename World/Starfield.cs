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

			Vector3 Pos = new Vector3(
				(float)Rand.NextDouble()/2f * Rand.RandomSign(),
				(float)Rand.NextDouble()/2f,
				(float)Rand.NextDouble()/2f * Rand.RandomSign()
			);
			Pos = Pos.Normalized() * 50000;

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
