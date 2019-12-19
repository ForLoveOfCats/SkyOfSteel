using Godot;
using System;
using Optional;
using static Pathfinding;



public class SlimeMob : Mob
{
	public const float AccelerationTime = 0.3f;
	public const float DecelerationTime = 0.2f;

	protected override float TopSpeed {
		get {
			return 40;
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


	private void UpdateTargetPoint(PointData Closest)
	{
		int Count = Closest.Friends.Count;
		if(Count > 0)
			TargetPoint = Closest.Friends[RandomInstance.Next(Count)].Some();
		else
			TargetPoint = PointData.None();
	}


	public override void CalcWants(Option<Tile> MaybeFloor)
	{
		MaybeFloor.Match(
			some: Floor =>
			{
				TargetPoint.Match(
					some: Target => {},

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
