using Godot;


public class DamageIndicator : TextureRect
{
	Vector2 ShotFirePosition;
	float OriginalRotation;
	public override void _Ready()
	{
		// Start transparency Tween in _Ready so we know the Tween has been created
		Tween TransparencyTween = GetNode<Tween>("Tween");
		// Tween the "modulate" property
		TransparencyTween.InterpolateProperty(this, "modulate", new Color(255, 255, 255, 1), new Color(255, 255, 255, 0), 3, Tween.TransitionType.Linear, Tween.EaseType.InOut);
		TransparencyTween.Start();
	}


	public void SetShotPosition(Vector3 ShotFireArg)
	{
		// Flatted the ShotFirePosition
		ShotFirePosition = new Vector2(ShotFireArg.x, ShotFireArg.z);
		OriginalRotation = Game.PossessedPlayer.Rotation.y;
	}


	public override void _Process(float delta)
	{
		// Calculate our position on a 2D plane
		Vector2 OurPoint = new Vector2(Game.PossessedPlayer.Translation.x, Game.PossessedPlayer.Translation.z);
		RectRotation = Mathf.Rad2Deg(OurPoint.AngleToPoint(ShotFirePosition) + OriginalRotation);
	}


	public void TweenComplete()
	{
		// We're now totally transparent, so time to
		QueueFree();
	}
}
