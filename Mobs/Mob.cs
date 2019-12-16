using Godot;
using Optional;
using static Godot.Mathf;
using static SteelMath;
using static Pathfinding;



public abstract class Mob : KinematicBody, IPushable
{
	private const float Gravity = 75f;
	private const float MaxFallSpeed = 80f;
	private const float Friction = 120f;

	protected abstract float TopSpeed { get; }
	protected abstract float Acceleration { get; }

	public Vector3 Momentum = new Vector3();
	public Option<PointData> TargetPoint;
	public Option<PointData> StartPoint;


	public virtual void CalcWants(Option<Tile> MaybeFloor)
	{}


	public void ApplyPush(Vector3 Push)
	{
		Momentum += Push;
	}


	public override void _Process(float Delta)
	{
		Option<Tile> Floor = Tile.None();
		{
			PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
			var Excluding = new Godot.Collections.Array{this};
			var Results = State.IntersectRay(Translation, Translation + new Vector3(0,-3,0), Excluding, 4);
			if(Results.Count > 0)
			{
				if(Results["collider"] is Tile Branch && Branch.Point != null)
					Floor = Branch.Some();
			}
		}

		StartPoint.Match(
			some: Start =>
			{
				var Closest = World.Pathfinder.GetClosestPoint(Translation);

				TargetPoint.Match(
					some: Target =>
					{
						if(Closest != Start && Closest != Target)
						{
							TargetPoint = PointData.None();
							StartPoint = Closest.Some();
						}
					},

					none: () =>
					{
						if(Closest != Start)
						{
							TargetPoint = PointData.None();
							StartPoint = Closest.Some();
						}
					}
				);
			},

			none: () =>
			{
				StartPoint = World.Pathfinder.GetClosestPoint(Translation).Some();
			}
		);

		CalcWants(Floor);

		if(IsOnFloor())
		{
			TargetPoint.Match(
				some: Target =>
				{
					// World.DebugPlot(Target.Pos, 0.1f);
					//Apply push toward TargetPoint but don't go to fast
					Momentum += ClampVec3(
						Target.Pos.Flattened() - Translation.Flattened(),
						Acceleration*Delta + Friction*Delta,
						Acceleration*Delta + Friction*Delta
					);
					Vector3 Temp = ClampVec3(Momentum.Flattened(), 0, TopSpeed);
					Momentum.x = Temp.x;
					Momentum.z = Temp.z;
				},

				none: () => {}
			);

			//Friction
			Vector3 Horz = Momentum.Flattened();
			Horz = Horz.Normalized() * Clamp(Horz.Length() - Friction*Delta, 0, TopSpeed);
			Momentum.x = Horz.x;
			Momentum.z = Horz.z;

			if(Momentum.y < 0)
				Momentum.y = 0;
		}
		else //Not on floor
		{
			//Gravity
			Momentum.y -= Gravity*Delta;
			if(Momentum.y < -MaxFallSpeed)
				Momentum.y = -MaxFallSpeed;
		}

		Momentum = MoveAndSlide(Momentum, floorNormal:new Vector3(0,1,0), floorMaxAngle:Deg2Rad(70));
	}
}
