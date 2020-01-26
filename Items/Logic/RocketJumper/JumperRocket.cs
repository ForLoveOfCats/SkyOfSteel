using Godot;
using System.Collections.Generic;
using static Godot.Mathf;
using static SteelMath;



public class JumperRocket : Spatial, IProjectileCollision
{
	public int FirerId { get; set; } //The player which fired the rocket, to prevent colliding fire-er
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
	public void Explode(Vector3 Position)
	{
		foreach(Node Body in AffectedBodies)
		{
			if(Body is IPushable Pushable)
			{
				PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
				Godot.Collections.Dictionary Results = State.IntersectRay(Position, Pushable.Translation, new Godot.Collections.Array(){Pushable}, 1);
				if(Results.Count > 0)
					continue;

				float Distance = Clamp(Position.DistanceTo(Pushable.Translation), 1, RocketJumper.MaxRocketDistance);
				float Power =
					LogBase(-Distance + RocketJumper.MaxRocketDistance + 1, 2)
					/ LogBase(RocketJumper.MaxRocketDistance + 1, 2);

				Vector3 Push = ((Pushable.Translation - Position) / RocketJumper.MaxRocketDistance).Normalized()
					* RocketJumper.MaxRocketPush * Power;
				{
					Vector3 Flat = Push.Flattened();
					Flat *= RocketJumper.RocketHorizontalMultiplyer;
					Push.x = Flat.x;
					Push.z = Flat.z;
				}
				Push.y *= RocketJumper.RocketVerticalMultiplyer;
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
		//No need to check for collisions
		//ProjectileCollision will do that on the server
		Translation += Momentum * Delta;

		if(!Net.Work.IsNetworkServer())
			return;

		if(Triggered || (Life >= RocketJumper.RocketFuseTime))
		{
			if(TriggeredPosition == null)
				TriggeredPosition = GetNode<Spatial>("ExplosionOrigin").GlobalTransform.origin;
			var Position = (Vector3) TriggeredPosition;

			Explode(Position);
			Net.SteelRpc(this, nameof(Explode), Position);
		}
		else
			Life += Delta;
	}
}
