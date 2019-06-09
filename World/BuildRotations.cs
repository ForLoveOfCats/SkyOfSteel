using Godot;
using static System.Math;
using static SteelMath;


public class BuildRotations //NOTE: This whole file is going away in 0.1.3
{
	private static Vector3 PlatformBranch(Structure Base)
	{
		//No need for a switch statement, all platforms should have rotation of 0,0,0
		return new Vector3();
	}


	private static Vector3 WallBranch(Structure Base)
	{
		if(Base.Type == Items.ID.WALL)
		{
			return Base.RotationDegrees;
		}

		return new Vector3(0, SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4), 0);
	}


	private static Vector3 SlopeBranch(Structure Base)
	{
		int BuildRotation = Game.PossessedPlayer.BuildRotation;
		int PlayerDirection = (int)LoopRotation(SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4));

		if(BuildRotation == 0)
		{
			return new Vector3(0, SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4), 0);
		}
		else
		{
			return new Vector3(0, LoopRotation(SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4)+180), 0);
		}
	}


	public static Vector3 Calculate(Structure Base, Items.ID BranchType)
	{
		switch(BranchType)
		{
			case(Items.ID.PLATFORM):
				return PlatformBranch(Base);

			case(Items.ID.WALL):
				return WallBranch(Base);

			case(Items.ID.SLOPE):
				return SlopeBranch(Base);

			default:
				return new Vector3();
		}
	}
}
