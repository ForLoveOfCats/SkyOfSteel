using Godot;


public class Structure : StaticBody
{
	public Items.TYPE Type = Items.TYPE.ERROR;
	public int OwnerId = 0;


	public void Remove()
	{
		if(Type == Items.TYPE.PLATFORM && Translation == new Vector3(0,0,0))
		{
			return; //Prevents removing the origin platform
		}

		if(ShouldDo.StructureRemove(Type, Translation, RotationDegrees, OwnerId))
		{
			Net.SteelRpc(Building.Self, nameof(Building.Remove), GetName());
			Building.Self.Remove(GetName());
		}
	}
}
