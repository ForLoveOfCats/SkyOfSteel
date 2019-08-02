using Godot;


public class DamageIndicator : TextureRect
{
	Vector3 ShotFirePosition;
	Vector2 ShotFirePos2D;
	float PlayerStartRotation;


	public override void _Ready()
	{
		Tween TransparencyTween = GetNode<Tween>("Tween");
		TransparencyTween.InterpolateProperty(this, "modulate", new Color(255, 255, 255, 1), new Color(255, 255, 255, 0), 3, Tween.TransitionType.Linear, Tween.EaseType.InOut);
		TransparencyTween.Start();
	}


	public void SetShotPosition(Vector3 ShotFirePositionArg)
	{
		ShotFirePosition = ShotFirePositionArg;
		ShotFirePos2D = new Vector2(ShotFirePositionArg.x, ShotFirePositionArg.z);
		PlayerStartRotation = Game.PossessedPlayer.Rotation.y;
	}


	public override void _Process(float delta)
	{
		Vector2 PlayerPosition2D = new Vector2(Game.PossessedPlayer.Translation.x, Game.PossessedPlayer.Translation.z);
		RectRotation = Mathf.Rad2Deg(PlayerPosition2D.AngleToPoint(ShotFirePos2D) + PlayerStartRotation);
	}


	public void TweenComplete()
	{
		// We're now totally transparent, so time to
		QueueFree();
	}
}
