using Godot;


public class BuildPositions
{
	private static System.Nullable<Vector3> PlatformBranch(Structure Base)
	{
		switch(Base.Type)
		{
			case(Items.TYPE.PLATFORM):
				float RotationDegrees = Mathf.Deg2Rad(SteelMath.SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4));
				Vector3 Position = Base.Translation + (new Vector3(0,0,12)).Rotated(new Vector3(0,1,0), RotationDegrees);
				return new Vector3(Mathf.Round(Position.x), Mathf.Round(Position.y), Mathf.Round(Position.z));


			default:
				return null;
		}
	}


	public static System.Nullable<Vector3> Calculate(Structure Base, Items.TYPE BranchType)
	{
		switch(BranchType)
		{
			case(Items.TYPE.PLATFORM):
				return PlatformBranch(Base);


			default:
				//Return null if unsuported, will be caught by Building.Request
				return null;
		}
	}
}
