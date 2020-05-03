using Godot;
using static Godot.Mathf;
using static SteelMath;
using Optional.Unsafe;



public class JumperRocket : Spatial, IProjectile {
	public const float RocketTravelSpeed = 150; //Units-per-second
	public const float RocketFuseTime = 4f; //In seconds
	public const float MaxRocketPush = 80; //Units-per-second force applied
	public const float MaxRocketDistance = 30;
	public const float RocketHorizontalMultiplyer = 1f;
	public const float RocketVerticalMultiplyer = 0.75f;

	public System.Tuple<int, int> CurrentChunk { get; set; }
	public Projectiles.ProjectileID ProjectileId { get; set; }
	public int FirerId { get; set; } //The player which fired the rocket, to prevent colliding fire-er
	public Vector3 Momentum { get; set; }
	public float Life = 0;
	public bool Triggered = false;
	public Vector3? TriggeredPosition = null;


	public static PackedScene ExplodeSfx;
	public static PackedScene ExplodeParticles;

	static JumperRocket() {
		ExplodeSfx = GD.Load<PackedScene>("Items/Logic/RocketJumper/ExplodeSfx.tscn");
		ExplodeParticles = GD.Load<PackedScene>("Items/Logic/RocketJumper/ExplosionParticles.tscn");
	}


	public override void _Ready() {
		World.AddEntityToChunk(this);
	}


	public override void _ExitTree() {
		World.RemoveEntityFromChunk(this);
	}


	public void ProjectileCollided(Vector3 CollisionPointPosition) {
		Triggered = true;
		TriggeredPosition = CollisionPointPosition;
	}


	[Remote]
	public void Update(params object[] Args) {
		Assert.ArgArray(Args, typeof(Vector3));
		var OriginalChunkTuple = World.GetChunkTuple(Translation);
		Translation = (Vector3)Args[0];
		Entities.MovedTick(this, OriginalChunkTuple);
	}


	[Remote]
	public void PhaseOut() {
		QueueFree();
	}


	[Remote]
	public void Destroy(params object[] Args) {
		Assert.ArgArray(Args, typeof(Vector3));
		ExplodeSoundVisual((Vector3)Args[0]);
		QueueFree();
	}


	public static void ExplodeSoundVisual(Vector3 Position) {
		var ExplodeSfxInstance = (AudioStreamPlayer3D)ExplodeSfx.Instance();
		ExplodeSfxInstance.Play();
		ExplodeSfxInstance.Translation = Position;
		World.EntitiesRoot.AddChild(ExplodeSfxInstance);

		var ParticleSystem = (CPUParticles)ExplodeParticles.Instance();
		ParticleSystem.Translation = Position;
		ParticleSystem.Emitting = true;
		World.EntitiesRoot.AddChild(ParticleSystem);
	}


	public static Vector3 CalculatePush(IPushable Pushable, Vector3 Position) {
		float Distance = Clamp(Position.DistanceTo(Pushable.Translation), 1, MaxRocketDistance);
		float Power =
			LogBase(-Distance + MaxRocketDistance + 1, 2)
			/ LogBase(MaxRocketDistance + 1, 2);

		Vector3 Push = ((Pushable.Translation - Position) / MaxRocketDistance).Normalized() * MaxRocketPush * Power;
		{
			Vector3 Flat = Push.Flattened();
			Flat *= RocketHorizontalMultiplyer;
			Push.x = Flat.x;
			Push.z = Flat.z;
		}
		Push.y *= RocketVerticalMultiplyer;

		return Push;
	}


	public void ServerExplode(Vector3 Position) {
		Assert.ActualAssert(Net.Work.IsNetworkServer());

		var WithinArea = World.GetEntitiesWithinArea(Position, MaxRocketDistance);
		foreach(Node Body in WithinArea) {
			if(Body is IPushable Pushable) {
				PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
				Godot.Collections.Dictionary Results = State.IntersectRay(Position, Pushable.Translation, new Godot.Collections.Array() { Pushable }, 1);
				if(Results.Count > 0)
					continue;

				Vector3 Push = CalculatePush(Pushable, Position);

				if(Pushable is Player Plr
					&& Game.PossessedPlayer.HasValue
					&& Plr != Game.PossessedPlayer.ValueOrFailure())
					continue;

				Pushable.ApplyPush(Push);
				Entities.SendPush(Pushable, Push);
			}
		}

		ExplodeSoundVisual(Position);

		Entities.SendDestroy(Name, Position);
		QueueFree();
	}


	public override void _PhysicsProcess(float Delta) {
		if(Triggered || (Life >= RocketFuseTime)) {
			if(TriggeredPosition == null)
				TriggeredPosition = GetNode<Spatial>("ExplosionOrigin").GlobalTransform.origin;
			var Position = (Vector3)TriggeredPosition;

			if(Net.Work.IsNetworkServer())
				ServerExplode(Position);
			else //Client
			{
				Game.PossessedPlayer.MatchSome(
					(Plr) => {
						if(Position.DistanceTo(Plr.Translation) <= MaxRocketDistance) {
							Vector3 Push = CalculatePush(Plr, Position);
							Plr.ApplyPush(Push);
						}
					}
				);

				ExplodeSoundVisual(Position);
				QueueFree();
			}

			return;
		}
		else
			Life += Delta;

		var OriginalChunkTuple = World.GetChunkTuple(Translation);
		Translation += Momentum * Delta;

		if(!Net.Work.IsNetworkServer())
			return;

		Entities.MovedTick(this, OriginalChunkTuple);

		Entities.AsServerMaybePhaseOut(this);
		Entities.SendUpdate(Name, Translation);
	}
}
