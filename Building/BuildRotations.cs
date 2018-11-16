using Godot;


public class BuildRotations
{
	private void MissingCaseError(Structure Base, Items.TYPE BranchType)
	{
		Console.Log("ERROR: Missing rotation case for branch '" + BranchType.ToString() + "' on base '" + Base.Type.ToString() + "'");
	}


	private Vector3 PlatformBranch(Structure Base, Items.TYPE BranchType)
	{
		switch(Base.Type)
		{
			case(Items.TYPE.PLATFORM):
				return new Vector3(0,0,0);


			default:
				MissingCaseError(Base, BranchType);
				return Base.RotationDegrees;
		}
	}


	public Vector3 Calculate(Structure Base, Items.TYPE BranchType)
	{
		switch(BranchType)
		{
			case(Items.TYPE.PLATFORM):
				return PlatformBranch(Base, BranchType);


			default:
				Console.Log("ERROR: Missing rotation switch for branch '" + BranchType.ToString() + "'");
				return Base.RotationDegrees;
		}
	}
}
