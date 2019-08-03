using Godot;


public class DamageIndicator : Sprite
{
	Vector3 ShotFirePosition;
	Vector2 ShotFirePos2D;
	float PlayerStartRotation;
	float MaxLife;
	float RemainingLife;
	public static Shader TransparencyShader;
	ShaderMaterial Mat;

	static DamageIndicator()
	{
		TransparencyShader = GD.Load<Shader>("res://UI/DamageIndicator.shader");
	}


	public void Setup(Vector3 ShotFirePositionArg, float MaxLifeArg)
	{
		ShotFirePosition = ShotFirePositionArg;
		ShotFirePos2D = new Vector2(ShotFirePositionArg.x, ShotFirePositionArg.z);
		PlayerStartRotation = Game.PossessedPlayer.Rotation.y;
		MaxLife = MaxLifeArg;
		RemainingLife = MaxLife;
		Mat = new ShaderMaterial();
		Mat.Shader = TransparencyShader;
		Mat.SetShaderParam("alpha", 1);
		Material = Mat;
		CenterSprite();
	}

	private void CenterSprite()
	{
		GlobalPosition = OS.GetWindowSize() / new Vector2(2, 2);
	}

	public override void _Process(float Delta)
	{
		Vector2 PlayerPosition2D = new Vector2(Game.PossessedPlayer.Translation.x, Game.PossessedPlayer.Translation.z);
		float Rotation = PlayerPosition2D.AngleToPoint(ShotFirePos2D) + PlayerStartRotation;
		SetRotation(Rotation);
		RemainingLife -= Delta;
		Mat.SetShaderParam("alpha", RemainingLife/MaxLife);
		if(RemainingLife <= 0)
		{
			SetRotation(Rotation);
			QueueFree();
			SetRotation(Rotation);
		}
		CenterSprite();
	}
}
