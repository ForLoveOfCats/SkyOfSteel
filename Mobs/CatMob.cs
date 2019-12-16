using Godot;
using Optional;
using static Pathfinding;



public class CatMob : Mob
{
	protected override float TopSpeed {
		get {
			return 30;
		}
	}
	protected override float Acceleration {
		get {
			return 95;
		}
	}

	private void UpdateTargetPointFollowPlayer(PointData Closest)
	{
		var Target = World.Pathfinder.GetClosestPoint(Game.PossessedPlayer.Translation);
		var Path = World.Pathfinder.PlotPath(Closest, Target);
		if(Path.Count >= 1)
			TargetPoint = Path.Last().Some();
		else
			TargetPoint = PointData.None();
	}


	public override void CalcWants(Option<Tile> MaybeFloor)
	{
		MaybeFloor.Match(
			some: Floor =>
			{
				PointData Closest = Floor.Point;
				TargetPoint.Match(
					some: Target =>
					{
						if(Closest == World.Pathfinder.GetClosestPoint(Game.PossessedPlayer.Translation))
							TargetPoint = PointData.None();

						else if(Closest == Target)
						{
							if(Target.Pos.Flattened().DistanceTo(Translation.Flattened()) <= 2)
								UpdateTargetPointFollowPlayer(Closest);
						}
					},

					none: () =>
					{
						if(Closest != World.Pathfinder.GetClosestPoint(Game.PossessedPlayer.Translation))
							UpdateTargetPointFollowPlayer(Closest);
					}
				);
			},

			none: () => {}
		);
	}
}
