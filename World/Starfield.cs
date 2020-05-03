using Godot;
using System;
using static Godot.Mathf;



public class Starfield : Spatial {
	private MeshInstance SingleStar;

	private static PackedScene StarScene;

	static Starfield() {
		if(Engine.EditorHint) { return; }

		StarScene = GD.Load<PackedScene>("res://World/Star.tscn");
	}


	public override void _Ready() {
		for(int i = 0; i < 1000; i++) {
			SingleStar = (MeshInstance)StarScene.Instance();
			AddChild(SingleStar);

			Vector3 Pos = new Vector3(
				(float)Game.Rand.NextDouble() / 2f * Game.Rand.RandomSign(),
				(float)Game.Rand.NextDouble() / 2f,
				(float)Game.Rand.NextDouble() / 2f * Game.Rand.RandomSign()
			);

			float Distance = 43_000f + (float)(Game.Rand.NextDouble() * 15_000d) * Game.Rand.RandomSign();
			Pos = Pos.Normalized() * Distance;

			SingleStar.Translation = Pos;
		}
	}


	public override void _Process(float Delta) {
		Camera Cam = GetViewport().GetCamera();
		if(Cam != null)
			Translation = Cam.GlobalTransform.origin;

		SpatialMaterial Mat = ((SpatialMaterial)SingleStar.GetSurfaceMaterial(0));
		Color Old = Mat.AlbedoColor;

		if(World.TimeOfDay > 0 && World.TimeOfDay < 30f * World.DayNightMinutes) //Daytime
			Mat.AlbedoColor = new Color(Old.r, Old.g, Old.b, 0);
		else {
			float Power = 0;
			if(World.TimeOfDay < 45f * World.DayNightMinutes) {
				//After sunset
				Power = (World.TimeOfDay - (30f * World.DayNightMinutes)) / (18f * World.DayNightMinutes);
				Power = Clamp(Power, 0, 1);
			}
			else {
				//Before sunrise
				Power = Abs(World.TimeOfDay - (60f * World.DayNightMinutes)) / (18f * World.DayNightMinutes);
				Power = Clamp(Power, 0, 1);
			}

			Mat.AlbedoColor = new Color(Old.r, Old.g, Old.b, Clamp(Power, 0, 1));
		}
	}
}
