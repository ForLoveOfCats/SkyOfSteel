using System;
using Godot;
using static SteelMath;



public class Mob : KinematicBody
{
	public static Random RandomInstance = new Random();

	public int TargetId = -1;


	public override void _Process(float Delta)
	{
		if(TargetId == -1 && World.Pathfinder.GetPointCount() <= 2)
			return;
		else if(TargetId == -1)
			TargetId = (int)World.Pathfinder.GetPoints()[RandomInstance.Next(World.Pathfinder.GetPointCount())];

		int ClosestId = World.Pathfinder.GetClosestPoint(Translation);
		if(ClosestId == TargetId)
		{
			TargetId = -1;
			return;
		}

		int[] Path = World.Pathfinder.GetIdPath(ClosestId, TargetId);
		if(Path.Length >= 2)
		{
			Vector3 SubTargetVec = World.Pathfinder.GetPointPosition(Path[1]);
			MoveAndSlide(
				ClampVec3(
					SubTargetVec-Translation,
					32,
					32
					)
			);
		}
		else
			TargetId = -1;
	}
}
