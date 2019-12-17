using Godot;
using Optional;
using System;
using static Godot.Mathf;
using static SteelMath;
using static Pathfinding;



public abstract class Mob : KinematicBody, IInGrid, IPushable
{
	private const float Gravity = 75f;
	private const float MaxFallSpeed = 80f;

	protected abstract float TopSpeed { get; }
	protected abstract float Acceleration { get; }
	protected abstract float Friction { get; }

	protected abstract Vector3 Bottom { get; }

	public Vector3 Momentum = new Vector3();
	public Option<PointData> TargetPoint = PointData.None();
	public Option<Tile> Floor = Tile.None();
	public Vector3 CurrentArea = new Vector3();


	public virtual void CalcWants(Option<Tile> MaybeFloor)
	{}


	public override void _Ready()
	{
		World.Grid.AddItem(this);
		UpdateFloor();
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
		Vector3 End = Translation + Bottom + new Vector3(0, -1, 0);
		var Results = State.IntersectRay(Translation, End, Excluding, 4);
		if(Results.Count > 0)
		{
			if(Results["collider"] is Tile Branch && Branch.Point != null)
				Floor = Branch.Some();
		}
	}


	public override void _Process(float Delta)
	{
		if(CurrentArea != GridClass.CalculateArea(Translation))
		{
			CurrentArea = GridClass.CalculateArea(Translation);
			World.Grid.QueueRemoveItem(this);
			World.Grid.AddItem(this);

			UpdateFloor();
		}

		CalcWants(Floor);

		if(IsOnFloor())
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
						Vector3 Temp = ClampVec3(Momentum.Flattened(), 0, TopSpeed);
						Momentum.x = Temp.x;
						Momentum.z = Temp.z;
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
		{
			//Gravity
			Momentum.y -= Gravity*Delta;
			if(Momentum.y < -MaxFallSpeed)
				Momentum.y = -MaxFallSpeed;
		}

		if(Momentum.y <= 0)
		{
			Vector3 SnapPoint = Bottom + new Vector3(0, -2, 0);
			Momentum = MoveAndSlideWithSnap(Momentum, SnapPoint, floorNormal:new Vector3(0,1,0), stopOnSlope:true, floorMaxAngle:Deg2Rad(70));
		}
		else
			Momentum = MoveAndSlide(Momentum, floorNormal:new Vector3(0,1,0), stopOnSlope:true, floorMaxAngle:Deg2Rad(70));
	}
}
