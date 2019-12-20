using Godot;
using System.Collections.Generic;


//A representation of a single world chunk
public class ChunkClass
{
	public List<Tile> Tiles = new List<Tile>();
	public List<MobClass> Mobs = new List<MobClass>();
	public List<DroppedItem> Items = new List<DroppedItem>();
}

