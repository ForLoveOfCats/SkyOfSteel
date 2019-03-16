using Godot;


public class Structure : StaticBody, IInGrid
{
	public Items.TYPE Type = Items.TYPE.ERROR;
	public int OwnerId = 0;


	public void GridUpdate()
	{}


	public void Remove(bool Force=false)
	{
		if(!Force && OwnerId == 0)
		{
			return; //Prevents removing default structures
		}

		if(ShouldDo.StructureRemove(Type, Translation, RotationDegrees, OwnerId))
		{
			Net.SteelRpc(World.Self, nameof(World.Remove), GetName());
			World.Self.Remove(GetName());
		}
	}
}
