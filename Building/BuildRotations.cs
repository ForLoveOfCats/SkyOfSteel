using Godot;


public class BuildRotations
{
	private void MissingCaseError(Structure Base, Items.TYPE Branch)
	{
		Console.Log("ERROR: Missing rotation case for '" + Branch.ToString() + "' *ON* '" + Base.Type.ToString() + "'");
	}


	private Vector3 OnPlatform(Structure Base, Items.TYPE Branch)
	{
		switch(Branch)
		{
			case(Items.TYPE.PLATFORM):
				return new Vector3(0,0,0);


			default:
				MissingCaseError(Base, Branch);
				return Base.RotationDegrees;
		}
	}


	public Vector3 Calculate(Structure Base, Items.TYPE Branch)
	{
		switch(Base.Type)
		{
			case(Items.TYPE.PLATFORM):
				return OnPlatform(Base, Branch);


			default:
				Console.Log("ERROR: Missing rotation switch for '" + Base.Type.ToString() + "' *AS* base");
				return Base.RotationDegrees;
		}
	}
}
