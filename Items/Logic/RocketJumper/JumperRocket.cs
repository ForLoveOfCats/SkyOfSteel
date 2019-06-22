using Godot;
using static Godot.Mathf;
using System.Collections.Generic;


public class JumperRocket : KinematicBody
{
	public bool IsLocal;
	public Node Player; //The player which fired the rocket, to prevent collinding fire-er
	public HashSet<Node> AffectedBodies = new HashSet<Node>();
	public Vector3 Momentum;
	public float Life = 0;
	public bool Triggered = false;


	public static PackedScene ExplodeSfx;

	static JumperRocket()
	{
		ExplodeSfx = GD.Load<PackedScene>("Items/Logic/RocketJumper/ExplodeSfx.tscn");
	}


	public void HasCollided(Node Collided)
	{
		if(IsLocal)
		{
			if(Collided != Player)
				Triggered = true;
		}
	}


	public void EffectAreaEntered(Node Body)
	{
		if(IsLocal)
		{
			if(Body == Player)
				return;
		}

		if(!AffectedBodies.Contains(Body))
			AffectedBodies.Add(Body);
	}


	public void EffectAreaExited(Node Body)
	{
		if(IsLocal)
		{
			if(Body == Player)
				return;
		}

		if(AffectedBodies.Contains(Body))
			AffectedBodies.Remove(Body);
	}


	[Remote]
	public void Explode()
	{
		if(IsLocal)
		{
			if((Player as Spatial).Translation.DistanceTo(Translation) <= RocketJumper.MaxRocketDistance)
				AffectedBodies.Add(Player);
		}

		foreach(Node _Body in AffectedBodies)
		{
			if(_Body is IPushable Body)
			{
				float Distance = Clamp(Translation.DistanceTo(Body.Translation) - RocketJumper.MinRocketDistance, 1, RocketJumper.MaxRocketDistance);
				float Power = RocketJumper.MaxRocketDistance / Distance / RocketJumper.MaxRocketDistance;

				Vector3 Push = ((Body.Translation - Translation) / RocketJumper.MaxRocketDistance).Normalized()
					* RocketJumper.MaxRocketPush * Power;
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

		AudioStreamPlayer3D ExplodeSfxInstance = ExplodeSfx.Instance() as AudioStreamPlayer3D;
		ExplodeSfxInstance.Play();
		ExplodeSfxInstance.Translation = Translation;
		World.EntitiesRoot.AddChild(ExplodeSfxInstance);

		QueueFree();
	}


	public override void _PhysicsProcess(float Delta)
	{
		if(IsLocal)
		{
			if(Triggered || (Life >= RocketJumper.RocketFuseTime))
			{
				Explode();
				Net.SteelRpc(this, nameof(Explode));
			}
		}

		MoveAndCollide(Momentum * Delta);
		Life += Delta;
	}
}
