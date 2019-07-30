using Godot;


public class DamageIndicator : TextureRect
{
    Vector3 ShotFirePosition;
	Vector2 FlatShotFirePos;
	float PlayerStartRotation;


	public override void _Ready()
	{
		// Start transparency Tween in _Ready so we know the Tween has been created
		Tween TransparencyTween = GetNode<Tween>("Tween");
		// Tween the "modulate" property
		TransparencyTween.InterpolateProperty(this, "modulate", new Color(255, 255, 255, 1), new Color(255, 255, 255, 0), 3, Tween.TransitionType.Linear, Tween.EaseType.InOut);
		TransparencyTween.Start();
	}


	public void SetShotPosition(Vector3 ShotFirePositionArg)
	{
        ShotFirePosition = ShotFirePositionArg;
		// Flatted the ShotFirePosition
		FlatShotFirePos = new Vector2(ShotFirePositionArg.x, ShotFirePositionArg.z);
		PlayerStartRotation = Game.PossessedPlayer.Rotation.y;
	}


	public override void _Process(float delta)
	{
		// Calculate our position on a 2D plane
		RectRotation = Mathf.Rad2Deg(new Vector2(Game.PossessedPlayer.Translation.x, Game.PossessedPlayer.Translation.z).AngleToPoint(FlatShotFirePos) + PlayerStartRotation);
	}


	public void TweenComplete()
	{
		// We're now totally transparent, so time to
		QueueFree();
	}
}
