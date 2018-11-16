using Godot;


public class BuildPositions
{
	private void MissingCaseError(Structure Base, Items.TYPE BranchType)
	{
		Console.Log("ERROR: Missing position case for branch '" + BranchType.ToString() + "' on base '" + Base.Type.ToString() + "'");
	}


	private Vector3 PlatformBranch(Structure Base, Items.TYPE BranchType)
	{
		switch(Base.Type)
		{
			case(Items.TYPE.PLATFORM):
				float RotationDegrees = Mathf.Deg2Rad(SteelMath.SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4));
				return Base.Translation + (new Vector3(0,0,6)).Rotated(new Vector3(0,1,0), RotationDegrees);


			default:
				MissingCaseError(Base, BranchType);
				return Base.Translation;
		}
	}


	public Vector3 Calculate(Structure Base, Items.TYPE BranchType)
	{
		switch(BranchType)
		{
			case(Items.TYPE.PLATFORM):
				return PlatformBranch(Base, BranchType);


			default:
				Console.Log("ERROR: Missing position switch for branch '" + BranchType.ToString() + "'");
				return Base.Translation;
		}
	}
}
