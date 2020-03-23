using Godot;
using System.Collections.Generic;


//A representation of a single world chunk
public class ChunkClass
{
	public List<Tile> Tiles = new List<Tile>();
	public List<MobClass> Mobs = new List<MobClass>();
	public List<DroppedItem> Items = new List<DroppedItem>();
	public List<IEntity> Entities = new List<IEntity>();


	public ChunkClass()
	{
		Tiles = new List<Tile>();
		Mobs = new List<MobClass>();
		Items = new List<DroppedItem>();
		Entities = new List<IEntity>();
	}


	public bool IsEmpty()
	{
		return Tiles.Count <= 0 && Mobs.Count <= 0 && Items.Count <= 0 && Mobs.Count <= 0;
	}
}

