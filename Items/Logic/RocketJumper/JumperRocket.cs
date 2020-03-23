using Godot;
using System.Collections.Generic;
using static Godot.Mathf;
using static SteelMath;



public class JumperRocket : Spatial, IProjectile
{
	public const float RocketTravelSpeed = 150; //Units-per-second
	public const float RocketFuseTime = 4f; //In seconds
	public const float MaxRocketPush = 72; //Units-per-second force applied
	public const float MaxRocketDistance = 30; //Make sure that radius of AffectArea on JumperRocket matches
	public const float RocketHorizontalMultiplyer = 1f;
	public const float RocketVerticalMultiplyer = 0.65f;

	public System.Tuple<int, int> CurrentChunk { get; set; }
	public Projectiles.ProjectileID ProjectileId { get; set; }
	public int FirerId { get; set; } //The player which fired the rocket, to prevent colliding fire-er
	public HashSet<Node> AffectedBodies = new HashSet<Node>();
	public Vector3 Momentum { get; set; }
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


	public override void _Ready()
	{
		World.AddEntityToChunk(this);
	}


	public override void _ExitTree()
	{
		World.RemoveEntityFromChunk(this);
	}


	public void ProjectileCollided(Vector3 CollisionPointPosition)
	{
		if(!Net.Work.IsNetworkServer())
			return;

		Triggered = true;
		TriggeredPosition = CollisionPointPosition;
	}


	public void EffectAreaEntered(Node Body)
	{
		if(!AffectedBodies.Contains(Body))
			AffectedBodies.Add(Body);
	}


	public void EffectAreaExited(Node Body)
	{
		if(AffectedBodies.Contains(Body))
			AffectedBodies.Remove(Body);
	}


	[Remote]
	public void Update(params object[] Args)
	{
		Assert.ArgArray(Args, typeof(Vector3));
		var OriginalChunkTuple = World.GetChunkTuple(Translation);
		Translation = (Vector3)Args[0];
		Entities.MovedTick(this, OriginalChunkTuple);
	}


	[Remote]
	public void PhaseOut()
	{
		QueueFree();
	}


	[Remote]
	public void Destroy(params object[] Args)
	{
		Assert.ArgArray(Args, typeof(Vector3));
		Explode((Vector3)Args[0]);
	}


	public void Explode(Vector3 Position)
	{
		foreach(Node Body in AffectedBodies)
		{
			if(Body is IPushable Pushable)
			{
				PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
				GD.Print($"Pushable with translation {Pushable.Translation}");
				Godot.Collections.Dictionary Results = State.IntersectRay(Position, Pushable.Translation, new Godot.Collections.Array(){Pushable}, 1);
				if(Results.Count > 0)
					continue;

				float Distance = Clamp(Position.DistanceTo(Pushable.Translation), 1, MaxRocketDistance);
				float Power =
					LogBase(-Distance + MaxRocketDistance + 1, 2)
					/ LogBase(MaxRocketDistance + 1, 2);

				Vector3 Push = ((Pushable.Translation - Position) / MaxRocketDistance).Normalized()
					* MaxRocketPush * Power;
				{
					Vector3 Flat = Push.Flattened();
					Flat *= RocketHorizontalMultiplyer;
					Push.x = Flat.x;
					Push.z = Flat.z;
				}
				Push.y *= RocketVerticalMultiplyer;
				Pushable.ApplyPush(Push);
			}
		}
		AffectedBodies.Clear();

		var ExplodeSfxInstance = (AudioStreamPlayer3D) ExplodeSfx.Instance();
		ExplodeSfxInstance.Play();
		ExplodeSfxInstance.Translation = Position;
		World.EntitiesRoot.AddChild(ExplodeSfxInstance);

		var ParticleSystem = (CPUParticles) ExplodeParticles.Instance();
		ParticleSystem.Translation = Position;
		ParticleSystem.Emitting = true;
		World.EntitiesRoot.AddChild(ParticleSystem);

		QueueFree();
	}


	public override void _PhysicsProcess(float Delta)
	{
		if(!Net.Work.IsNetworkServer())
			return;

		var OriginalChunkTuple = World.GetChunkTuple(Translation);

		if(Triggered || (Life >= RocketFuseTime))
		{
			if(TriggeredPosition == null)
				TriggeredPosition = GetNode<Spatial>("ExplosionOrigin").GlobalTransform.origin;
			var Position = (Vector3) TriggeredPosition;

			Explode(Position);
			Entities.SendDestroy(Name, Position);
			return;
		}
		else
			Life += Delta;

		Translation += Momentum * Delta;
		Entities.AsServerMaybePhaseOut(this);
		Entities.SendUpdate(Name, Translation);

		Entities.MovedTick(this, OriginalChunkTuple);
	}
}
