using Godot;
using System;
using Optional;
using static Pathfinding;



public class CatMob : Mob
{
	public const float AccelerationTime = 0.3f;
	public const float DecelerationTime = 0.2f;

	protected override float TopSpeed {
		get {
			return 45;
		}
	}
	protected override float Acceleration {
		get {
			return TopSpeed/AccelerationTime;
		}
	}
	protected override float Friction {
		get {
			return TopSpeed/DecelerationTime;
		}
	}

	protected override Vector3 Bottom {
		get {
			return new Vector3(0, -1f, 0);
		}
	}

	private static Random RandomInstance = new Random();
	private PointData Goal = null;


	private void UpdateTargetPoint(PointData Closest)
	{
		var Path = World.Pathfinder.PlotPath(Closest, Goal);
		if(Path.Count >= 1)
			TargetPoint = Path.Last().Some();
		else
			TargetPoint = PointData.None();
	}


	public override void CalcWants(Option<Tile> MaybeFloor)
	{
		if(Goal == null)
		{
			int Count = World.Pathfinder.Points.Count;
			Goal = World.Pathfinder.Points[RandomInstance.Next(Count)];
		}

		MaybeFloor.Match(
			some: Floor =>
			{
				TargetPoint.Match(
					some: Target =>
					{
						if(Floor.Point == Goal)
						{
							int Count = World.Pathfinder.Points.Count;
							Goal = World.Pathfinder.Points[RandomInstance.Next(Count)];
						}
					},

					none: () =>
					{
						UpdateTargetPoint(Floor.Point);
					}
				);
			},

			none: () => {}
		);
	}
}
