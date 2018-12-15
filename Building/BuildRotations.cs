using Godot;


public class BuildRotations
{
	private static Vector3 PlatformBranch(Structure Base)
	{
		//No need for a switch statement, all platforms should have rotation of 0,0,0
		return new Vector3();
	}


	public static Vector3 Calculate(Structure Base, Items.TYPE BranchType)
	{
		switch(BranchType)
		{
			case(Items.TYPE.PLATFORM):
				return PlatformBranch(Base);


			default:
				return new Vector3();
		}
	}
}
