using Godot;


public class BuildPositions
{
	private void MissingCaseError(Structure Base, Items.TYPE Branch)
	{
		Console.Log("ERROR: Missing position case for '" + Branch.ToString() + "' *ON* '" + Base.Type.ToString() + "'");
	}


	private Vector3 OnPlatform(Structure Base, Items.TYPE Branch)
	{
		switch(Branch)
		{
			case(Items.TYPE.PLATFORM):
				float RotationDegrees = Mathf.Deg2Rad(SteelMath.SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4));
				return Base.Translation + (new Vector3(0,0,6)).Rotated(new Vector3(0,1,0), RotationDegrees);


			default:
				MissingCaseError(Base, Branch);
				return Base.Translation;
		}
	}


	public Vector3 Calculate(Structure Base, Items.TYPE Branch)
	{
		switch(Base.Type)
		{
			case(Items.TYPE.PLATFORM):
				return OnPlatform(Base, Branch);


			default:
				Console.Log("ERROR: Missing position switch for '" + Base.Type.ToString() + "' *AS* base");
				return Base.Translation;
		}
	}
}
