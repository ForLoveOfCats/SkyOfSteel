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

	private void UpdateFollowPlayer(PointData Closest)
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
				TargetPoint.Match(
					some: Target =>
					{
						if(Floor.Point == World.Pathfinder.GetClosestPoint(Game.PossessedPlayer.Translation))
							TargetPoint = PointData.None();
					},

					none: () =>
					{
						if(Floor.Point != World.Pathfinder.GetClosestPoint(Game.PossessedPlayer.Translation))
							UpdateFollowPlayer(Floor.Point);
					}
				);
			},

			none: () => {}
		);
	}
}
