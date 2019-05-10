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

		if(Game.Mode.ShouldRemoveStructure(Type, Translation, RotationDegrees, OwnerId))
		{
			World.Self.RemoveStructure(GetName());
		}
	}


	public void NetRemove(bool Force=false)
	{
		if(!Force && OwnerId == 0)
		{
			return; //Prevents removing default structures
		}

		if(Game.Mode.ShouldRemoveStructure(Type, Translation, RotationDegrees, OwnerId))
		{
			Net.SteelRpc(World.Self, nameof(World.RemoveStructure), GetName());
			World.Self.RemoveStructure(GetName());
		}
	}
}
