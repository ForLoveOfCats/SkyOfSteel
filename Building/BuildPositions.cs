using Godot;


public class BuildPositions
{
	private System.Nullable<Vector3> PlatformBranch(Structure Base, Items.TYPE BranchType)
	{
		switch(Base.Type)
		{
			case(Items.TYPE.PLATFORM):
				float RotationDegrees = Mathf.Deg2Rad(SteelMath.SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4));
				return Base.Translation + (new Vector3(0,0,6)).Rotated(new Vector3(0,1,0), RotationDegrees);


			default:
				return null;
		}
	}


	public System.Nullable<Vector3> Calculate(Structure Base, Items.TYPE BranchType)
	{
		switch(BranchType)
		{
			case(Items.TYPE.PLATFORM):
				return PlatformBranch(Base, BranchType);


			default:
				//Return null if unsuported, will be caught by Building.Request
				return null;
		}
	}
}
