using Godot;
using System.Collections.Generic;
using static SteelMath;



public class Mob : KinematicBody
{
	public override void _Process(float Delta)
	{
		var Closest = World.Pathfinder.GetClosestPoint(Translation);
		var Target = World.Pathfinder.GetClosestPoint(Game.PossessedPlayer.Translation);
		List<Pathfinding.PointData> Path = World.Pathfinder.PlotPath(Closest, Target);
		if(Path.Count >= 1)
		{
			Vector3 SubTargetVec = Path.Last().Pos;
			MoveAndSlide(
				ClampVec3(
					SubTargetVec-Translation,
					32,
					32
					)
			);
		}
	}
}
