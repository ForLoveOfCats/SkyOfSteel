using System;
using Godot;
using static SteelMath;



public class Mob : KinematicBody
{
	public static Random RandomInstance = new Random();

	public float TimeUntilNextMovement = 0;
	public int TargetId = -1;


	public override void _Process(float Delta)
	{
		TimeUntilNextMovement -= Delta;
		if(TimeUntilNextMovement <= 0)
		{
			TimeUntilNextMovement = RandomInstance.Next(10, 15);

			if(World.Pathfinder.GetPointCount() <= 0)
				return;

			int ClosestId = World.Pathfinder.GetClosestPoint(Translation);

			int IterationCount = 0;
			int LocalTargetId = ClosestId;
			while(LocalTargetId == ClosestId)
			{
				IterationCount += 1;
				if(IterationCount > 10)
				{
					TimeUntilNextMovement = RandomInstance.Next(0, 1);
					return;
				}

				LocalTargetId = (int)World.Pathfinder.GetPoints()[RandomInstance.Next(World.Pathfinder.GetPointCount())];
			}
			TargetId = LocalTargetId;

			// int[] Path = World.Pathfinder.GetIdPath(ClosestId, TargetId);
			// GD.Print($"Closest: {ClosestId}, Closest connection count: {World.Pathfinder.GetPointConnections(ClosestId).Length}, TargetId: {TargetId}, Path count: {Path.Length}");
		}

		{
			if(TargetId == -1 || World.Pathfinder.GetPointCount() <= 2)
				return;

			int ClosestId = World.Pathfinder.GetClosestPoint(Translation);
			if(ClosestId == TargetId)
			{
				TimeUntilNextMovement = RandomInstance.Next(0, 1);
				TargetId = -1;
				return;
			}

			int[] Path = World.Pathfinder.GetIdPath(ClosestId, TargetId);
			if(Path.Length > 2)
			{
				Vector3 SubTargetVec = World.Pathfinder.GetPointPosition(Path[1]);
				MoveAndSlide(
					ClampVec3(
						SubTargetVec-Translation/* + new Vector3(
							((float)RandomInstance.Next(-40, 40))/5f,
							0,
							((float)RandomInstance.Next(-40, 40))/5f)*/,
						32,
						32
					)
				);
			}
		}
	}
}
