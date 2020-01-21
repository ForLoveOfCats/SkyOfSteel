using Godot;
using Optional;



public class Tile : StaticBody, IInGrid
{
	public Items.ID ItemId = Items.ID.ERROR;
	public int OwnerId = 0;
	public Pathfinding.PointData Point = null;


	public virtual void GridUpdate()
	{}


	public void Remove(bool Force=false)
	{
		if(!Force && OwnerId == 0)
		{
			return; //Prevents removing default structures
		}

		World.Self.RemoveTile(Name);
	}


	public virtual void OnRemove()
	{}


	public void NetRemove(bool Force=false)
	{
		if(!Force && OwnerId == 0)
		{
			return; //Prevents removing default structures
		}

		Net.SteelRpc(World.Self, nameof(World.RemoveTile), Name);
		World.Self.RemoveTile(Name);
	}


	public static Option<Tile> None()
	{
		return Option.None<Tile>();
	}


	public Option<Tile> Some()
	{
		return Option.Some(this);
	}
}
