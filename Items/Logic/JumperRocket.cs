using Godot;
using static Godot.Mathf;
using System.Collections.Generic;


public class JumperRocket : KinematicBody
{
	public Node Player; //The player which fired the rocket, to prevent collinding fire-er
	public HashSet<Node> AffectedBodies = new HashSet<Node>();
	public Vector3 Momentum;
	public float Life = 0;
	public bool Triggered = false;


	public void HasCollided(Node Collided)
	{
		if(Collided != Player)
			Triggered = true;
	}


	public void EffectAreaEntered(Node Body)
	{
		if(Body == Player)
			return;

		if(!AffectedBodies.Contains(Body))
			AffectedBodies.Add(Body);
	}


	public void EffectAreaExited(Node Body)
	{
		if(Body == Player)
			return;

		if(AffectedBodies.Contains(Body))
			AffectedBodies.Remove(Body);
	}


	public void Explode()
	{
		if((Player as Spatial).Translation.DistanceTo(Translation) <= RocketJumper.MaxRocketDistance)
			AffectedBodies.Add(Player);
		foreach(Node _Body in AffectedBodies)
		{
			if(_Body is IPushable Body)
			{
				float Distance = Body.Translation.DistanceTo(Body.Translation);
				float Power = (RocketJumper.MaxRocketPush/RocketJumper.MaxRocketDistance) *
					Clamp(RocketJumper.MaxRocketDistance - Distance, 0, RocketJumper.MaxRocketDistance);

				Vector3 Push = ((Body.Translation - Translation) / RocketJumper.MaxRocketDistance).Normalized() * Power;
				{
					Vector3 Flat = Push.Flattened();
					Flat *= RocketJumper.RocketHorizontalMultiplyer;
					Push.x = Flat.x;
					Push.z = Flat.z;
				}
				Push.y /= RocketJumper.RocketVerticalDivisor;
				Body.ApplyPush(Push);

				if(Body is Player AffectedPlayer)
					AffectedPlayer.RecoverPercentage = 0;
			}
		}
		AffectedBodies.Clear();

		QueueFree();
	}


	public override void _PhysicsProcess(float Delta)
	{
		if(Triggered && Life >= RocketJumper.RocketArmTime)
			Explode();
		else if(Life >= RocketJumper.RocketFuseTime)
			Explode();

		MoveAndCollide(Momentum * Delta);
		Life += Delta;
	}
}
