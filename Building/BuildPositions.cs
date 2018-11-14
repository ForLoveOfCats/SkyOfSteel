using Godot;


public class BuildPositions
{
	private Vector3 OnPlatform(Structure Base)
	{
		switch(Base.Type)
		{
			case(Items.TYPE.PLATFORM):
				return new Vector3();


			default:
				Console.Log("ERROR: Cannot build '" + Base.Type.ToString() + "' on 'PLATFORM'");
				return Base.Translation;
		}
	}


	public Vector3 Calculate(Structure Base)
	{
		switch(Base.Type)
		{
			case(Items.TYPE.PLATFORM):
				return OnPlatform(Base);

			default:
				Console.Log("ERROR: Cannot use '" + Base.Type.ToString() + "' as base");
				return Base.Translation;
		}
	}
}
