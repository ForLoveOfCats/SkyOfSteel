using Godot;
using static Godot.Mathf;
using static SteelMath;
using System.Collections.Generic;


public class JumperRocket : Spatial, IProjectileCollision
{
	public bool IsLocal;
	public Player FiringPlayer; //The player which fired the rocket, to prevent colliding fire-er
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
			if(Body == FiringPlayer)
				return;
		}

		if(!AffectedBodies.Contains(Body))
			AffectedBodies.Add(Body);
	}


	public void EffectAreaExited(Node Body)
	{
		if(IsLocal)
		{
			if(Body == FiringPlayer)
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

		Vector3 Origin = GetNode<Spatial>("ExplosionOrigin").GlobalTransform.origin;

		if(IsLocal)
		{
			if(FiringPlayer.Translation.DistanceTo(Origin) <= RocketJumper.MaxRocketDistance)
				AffectedBodies.Add(FiringPlayer);
		}

		foreach(Node _Body in AffectedBodies)
		{
			if(_Body is IPushable Body)
			{
				PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
				Godot.Collections.Dictionary Results = State.IntersectRay(Origin, Body.Translation, new Godot.Collections.Array(){Body}, 1);
				if(Results.Count > 0)
					continue;

				float Distance = Clamp(Origin.DistanceTo(Body.Translation), 1, RocketJumper.MaxRocketDistance);
				float Power =
					LogBase(-Distance + RocketJumper.MaxRocketDistance + 1, 2)
					/ LogBase(RocketJumper.MaxRocketDistance + 1, 2);

				Vector3 Push = ((Body.Translation - Origin) / RocketJumper.MaxRocketDistance).Normalized()
					* RocketJumper.MaxRocketPush * Power;
				{
					Vector3 Flat = Push.Flattened();
					Flat *= RocketJumper.RocketHorizontalMultiplyer;
					Push.x = Flat.x;
					Push.z = Flat.z;
				}
				Push.y *= RocketJumper.RocketVerticalMultiplyer;
				Body.ApplyPush(Push);
			}
		}
		AffectedBodies.Clear();

		var ExplodeSfxInstance = (AudioStreamPlayer3D) ExplodeSfx.Instance();
		ExplodeSfxInstance.Play();
		ExplodeSfxInstance.Translation = Translation;
		World.EntitiesRoot.AddChild(ExplodeSfxInstance);

		var ParticleSystem = (CPUParticles) ExplodeParticles.Instance();
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

		Translation += Momentum * Delta;
		Life += Delta;
	}
}
