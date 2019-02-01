using Godot;


public class Structure : StaticBody
{
	public Items.TYPE Type = Items.TYPE.ERROR;
	public int OwnerId = 0;


	public void Remove()
	{
		if(ShouldDo.StructureRemove(Type, Translation, RotationDegrees, OwnerId))
		{
			Building.Self.Rpc(nameof(Building.Remove), GetName());
			Building.Self.Remove(GetName());
		}
	}
}
