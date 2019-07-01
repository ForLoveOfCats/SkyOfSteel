using Godot;
using System.Collections.Generic;


//A representation of a single world chunk
public class ChunkClass
{
	public List<Tile> Structures = new List<Tile>();
	public List<DroppedItem> Items = new List<DroppedItem>();
}

