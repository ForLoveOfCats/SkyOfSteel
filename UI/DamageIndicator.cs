using Godot;


public class DamageIndicator : Sprite
{
	public float MaxLife;
	public float RemainingLife;

	public Vector2 ShotFirePos2D;

	public ShaderMaterial Mat;

	public static Shader TransparencyShader;

	static DamageIndicator()
	{
		TransparencyShader = GD.Load<Shader>("res://UI/DamageIndicator.shader");
	}


	public void Setup(Vector3 Origin, float MaxLifeArg)
	{
		MaxLife = MaxLifeArg;
		RemainingLife = MaxLife;

		ShotFirePos2D = new Vector2(Origin.x, Origin.z);

		Mat = new ShaderMaterial();
		Mat.Shader = TransparencyShader;
		Mat.SetShaderParam("alpha", 1);
		Material = Mat;

		CenterSprite();
	}


	private void CenterSprite()
	{
		GlobalPosition = OS.WindowSize / new Vector2(2, 2);
	}


	public override void _Process(float Delta)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				var PlayerPosition2D = new Vector2(Plr.Translation.x, Plr.Translation.z);
				Rotation = PlayerPosition2D.AngleToPoint(ShotFirePos2D) + Plr.Rotation.y;

				RemainingLife -= Delta;
				if(RemainingLife <= 0)
					QueueFree();

				Mat.SetShaderParam("alpha", RemainingLife / MaxLife);

				CenterSprite();
			}
		);
	}
}
