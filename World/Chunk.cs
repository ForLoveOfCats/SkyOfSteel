using Godot;
using System.Collections.Generic;


//A representation of a single world chunk
public class ChunkClass
{
	public List<Structure> Structures = new List<Structure>();
	private List<DroppedItem> _Items = new List<DroppedItem>();
	public List<DroppedItem> Items
	{
		get
		{
			return _Items;
		}
		set
		{
			if(_Items.Count > 1)
			{
				// throw new System.Exception("A second dropped item was added to this chunk");
			}
			_Items = value;
		}
	}

	public ChunkClass()
	{
		Items = new List<DroppedItem>();
	}
}

