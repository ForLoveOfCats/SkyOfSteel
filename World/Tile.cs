using Godot;


public class Tile : StaticBody, IInGrid
{
	public Items.ID Type = Items.ID.ERROR;
	public int OwnerId = 0;


	public void GridUpdate()
	{}


	public void Remove(bool Force=false)
	{
		if(!Force && OwnerId == 0)
		{
			return; //Prevents removing default structures
		}

		World.Self.RemoveStructure(GetName());
	}


	public void NetRemove(bool Force=false)
	{
		if(!Force && OwnerId == 0)
		{
			return; //Prevents removing default structures
		}

		Net.SteelRpc(World.Self, nameof(World.RemoveStructure), GetName());
		World.Self.RemoveStructure(GetName());
	}
}
