using Godot;
using Optional;
using System;
using static Godot.Mathf;
using static SteelMath;
using static Pathfinding;



public abstract class MobClass : Character, IEntity, IInGrid, IPushable
{
	private const float Gravity = 75f;
	private const float MaxFallSpeed = 80f;
	private const float MaxTimeSinceUpdate = 1f/30f;

	protected abstract float TopSpeed { get; }
	protected abstract float Acceleration { get; }
	protected abstract float Friction { get; }

	protected abstract Vector3 Bottom { get; }

	public Mobs.ID Type;

	public Vector3 Momentum = new Vector3();
	public Option<PointData> TargetPoint = PointData.None();
	public Option<Tile> Floor = Tile.None();
	public Vector3 CurrentArea = new Vector3();
	public System.Tuple<int, int> DepreciatedCurrentChunkTuple;
	public Tuple<int, int> CurrentChunk { get; set; } //TODO: This is for IEntity, unify

	float TimeSinceUpdate = 0;


	public virtual void CalcWants(Option<Tile> MaybeFloor)
	{}


	public override void _Ready()
	{
		World.AddEntityToChunk(this);

		if(!Net.Work.IsNetworkServer())
		{
			SetProcess(false);
			return;
		}

		World.Grid.AddItem(this);
		UpdateFloor();
	}


	public override void _ExitTree()
	{
		World.RemoveEntityFromChunk(this);
	}


	public void GridUpdate()
	{
		UpdateFloor();
	}


	public void ApplyPush(Vector3 Push)
	{
		Momentum += Push;
	}


	public void UpdateFloor()
	{
		PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
		var Excluding = new Godot.Collections.Array{this};
		Vector3 End = Translation + Bottom + new Vector3(0, -2, 0);
		var Results = State.IntersectRay(Translation, End, Excluding, 3);
		if(Results.Count > 0)
		{
			if(Results["collider"] is Tile Branch && Branch.Point != null)
				Floor = Branch.Some();
		}
	}


	public override void _Process(float Delta) //NOTE: Only runs on server
	{
		Assert.ActualAssert(Net.Work.IsNetworkServer());

		TimeSinceUpdate += Delta;

		if(!DepreciatedCurrentChunkTuple.Equals(World.GetChunkTuple(Translation)))
		{
			World.Chunks[DepreciatedCurrentChunkTuple].Mobs.Remove(this);
			World.AddMobToChunk(this);
		}

		if(CurrentArea != GridClass.CalculateArea(Translation))
		{
			CurrentArea = GridClass.CalculateArea(Translation);
			World.Grid.QueueRemoveItem(this);
			World.Grid.AddItem(this);
		}

		UpdateFloor();
		CalcWants(Floor);

		if(OnFloor)
		{
			TargetPoint.Match(
				some: Target =>
				{
					if(Target.Pos.Flattened().DistanceTo(Translation.Flattened()) <= 2)
						TargetPoint = PointData.None();
					else
					{
						//Apply push toward TargetPoint but don't go to fast
						Momentum += ClampVec3(
							Target.Pos.Flattened() - Translation.Flattened(),
							Acceleration*Delta + Friction*Delta,
							Acceleration*Delta + Friction*Delta
						);
						Vector3 Clamped = ClampVec3(Momentum.Flattened(), 0, TopSpeed);
						Momentum.x = Clamped.x;
						Momentum.z = Clamped.z;
					}
				},

				none: () => {}
			);

			//Friction
			Vector3 Horz = Momentum.Flattened();
			Horz = Horz.Normalized() * Clamp(Horz.Length() - Friction*Delta, 0, TopSpeed);
			Momentum.x = Horz.x;
			Momentum.z = Horz.z;
		}
		else //Not on floor
			Momentum.y = Clamp(Momentum.y - Gravity*Delta, -MaxFallSpeed, MaxFallSpeed); //Apply gravity

		var OriginalChunkTuple = World.GetChunkTuple(Translation);
		Momentum = Move(Momentum, Delta, 1, 60, TopSpeed);

		Entities.MovedTick(this, OriginalChunkTuple);

		Entities.AsServerMaybePhaseOut(this);
		Entities.SendUpdate(Name, Translation);
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
		Assert.ArgArray(Args);
		QueueFree();
	}
}
