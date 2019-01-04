using Godot;
using static SteelMath;


public class BuildRotations
{
	private static Vector3 PlatformBranch(Structure Base)
	{
		//No need for a switch statement, all platforms should have rotation of 0,0,0
		return new Vector3();
	}


	private static Vector3 WallBranch(Structure Base)
	{
		return new Vector3(0, SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4), 0);
	}


	public static Vector3 Calculate(Structure Base, Items.TYPE BranchType)
	{
		switch(BranchType)
		{
			case(Items.TYPE.PLATFORM):
				return PlatformBranch(Base);

			case(Items.TYPE.WALL):
				return WallBranch(Base);

			default:
				return new Vector3();
		}
	}
}
