using Godot;


public class Structure : StaticBody
{
	public Items.TYPE Type = Items.TYPE.ERROR;
	public int OwnerId = 0;


	public void Remove(bool Force=false)
	{
		if(!Force && OwnerId == 0)
		{
			return; //Prevents removing default structures
		}

		if(ShouldDo.StructureRemove(Type, Translation, RotationDegrees, OwnerId))
		{
			Net.SteelRpc(Building.Self, nameof(Building.Remove), GetName());
			Building.Self.Remove(GetName());
		}
	}
}
