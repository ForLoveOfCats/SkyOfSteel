using Godot;


public class Structure : StaticBody
{
	public Items.TYPE Type = Items.TYPE.ERROR;
	public int OwnerId = 0;

	Structure(Items.TYPE TypeArg, int OwnerIdArg)
	{
		Type = TypeArg;
		OwnerId = OwnerIdArg;
	}
}
