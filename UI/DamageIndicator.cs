using Godot;
using System;

public class DamageIndicator : TextureRect
{
    // Angle the bullet came from
    Vector2 ShotFirePosition;
    float OriginalAngle;
    public override void _Ready()
    {
        // Start transparency Tween in _Ready so we know the Tween has been created
        Tween TransparencyTween = GetNode<Tween>("Tween");
        // Tween the "modulate" property
        TransparencyTween.InterpolateProperty(this,"modulate", new Color(255,255,255,1), new Color(255,255,255,0), 3, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        TransparencyTween.Start();
    }


    public void SetTargetAngle(Vector2 ShotFirePosition)
    {
        // The rotation we're looking at is the Y rotation
        this.ShotFirePosition = ShotFirePosition;
        // Original angle
        OriginalAngle = Game.PossessedPlayer.Rotation.y;
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        // Calculate rotation
        // Calculate our position on a 2D plane
        Vector2 OurPoint = new Vector2(Game.PossessedPlayer.Translation.x, Game.PossessedPlayer.Translation.z);
        // Get our rotation by getting the angle from where we we're shot from, and adding where we we're looking before
        RectRotation = Mathf.Rad2Deg(OurPoint.AngleToPoint(ShotFirePosition)+OriginalAngle);
    }


    public void TweenComplete()
    {
        // We're now totally transparent, so time to
        QueueFree();
    }
}