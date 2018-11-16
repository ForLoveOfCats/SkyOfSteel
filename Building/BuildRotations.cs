using Godot;


public class BuildRotations
{
	private Vector3 PlatformBranch(Structure Base, Items.TYPE BranchType)
	{
		//No need for a switch statement, all platforms should have rotation of 0,0,0
		return new Vector3();
	}


	public Vector3 Calculate(Structure Base, Items.TYPE BranchType)
	{
		switch(BranchType)
		{
			case(Items.TYPE.PLATFORM):
				return PlatformBranch(Base, BranchType);


			default:
				return new Vector3();
		}
	}
}
