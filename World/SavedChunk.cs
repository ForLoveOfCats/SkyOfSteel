using Godot;
using System;
using System.Collections.Generic;



public class SavedChunk
{
	public Vector3 P;
	public List<SavedTile> Tiles = new List<SavedTile>();

	Tuple<int,int> ChunkTuple;

	public SavedChunk(Tuple<int,int> ChunkTupleArg)
	{
		ChunkTuple = ChunkTupleArg;
		P = new Vector3(ChunkTuple.Item1, 0, ChunkTuple.Item2);

		foreach(IEntity Entity in World.Chunks[ChunkTuple].Entities)
		{
			switch(Entity)
			{
				case Tile Branch:
					if(Branch.OwnerId != 0)
						Tiles.Add(Branch.ToSavable());
					break;
			}
		}
	}

	public SavedChunk()
	{}
}
