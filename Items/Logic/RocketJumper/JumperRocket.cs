using Godot;
using static Godot.Mathf;
using System.Collections.Generic;


public class JumperRocket : KinematicBody, IProjectileCollision
{
	public bool IsLocal;
	public Node Player; //The player which fired the rocket, to prevent collinding fire-er
	public HashSet<Node> AffectedBodies = new HashSet<Node>();
	public Vector3 Momentum;
	public float Life = 0;
	public bool Triggered = false;
	public Vector3? TriggeredPosition = null;


	public static PackedScene ExplodeSfx;
	public static PackedScene ExplodeParticles;

	static JumperRocket()
	{
		ExplodeSfx = GD.Load<PackedScene>("Items/Logic/RocketJumper/ExplodeSfx.tscn");
		ExplodeParticles = GD.Load<PackedScene>("Items/Logic/RocketJumper/ExplosionParticles.tscn");
	}


	public void ProjectileCollided(Vector3 CollisionPointPosition)
	{
		Triggered = true;
		TriggeredPosition = CollisionPointPosition;
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
		if(TriggeredPosition != null)
			Translation = (Vector3)TriggeredPosition;

		Vector3 Origin = GetNode<Spatial>("ExplosionOrigin").GetGlobalTransform().origin;

		if(IsLocal)
		{
			if((Player as Spatial).Translation.DistanceTo(Origin) <= RocketJumper.MaxRocketDistance)
				AffectedBodies.Add(Player);
		}

		foreach(Node _Body in AffectedBodies)
		{
			if(_Body is IPushable Body)
			{
				PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
				Godot.Collections.Dictionary Results = State.IntersectRay(Origin, Body.Translation, new Godot.Collections.Array(){Body}, 1);
				if(Results.Count > 0)
					continue;

				float Distance = Clamp(Origin.DistanceTo(Body.Translation) - RocketJumper.MinRocketDistance, 1, RocketJumper.MaxRocketDistance);
				float Power = RocketJumper.MaxRocketDistance / Distance / RocketJumper.MaxRocketDistance;

				Vector3 Push = ((Body.Translation - Origin) / RocketJumper.MaxRocketDistance).Normalized()
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

		CPUParticles ParticleSystem = ExplodeParticles.Instance() as CPUParticles;
		ParticleSystem.Translation = Translation;
		ParticleSystem.Emitting = true;
		World.EntitiesRoot.AddChild(ParticleSystem);

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
