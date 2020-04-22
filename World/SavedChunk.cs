using Godot;
using System;
using System.Collections.Generic;



public class SavedChunk
{
	public int[] P;
	public SavedTile[] S;

	Tuple<int,int> ChunkTuple;

	public SavedChunk(Tuple<int,int> ChunkTupleArg)
	{
		ChunkTuple = ChunkTupleArg;
		P = new int[2] {ChunkTuple.Item1, ChunkTuple.Item2};

		var Tiles = new List<SavedTile>();
		foreach(Tile Branch in World.Chunks[ChunkTuple].Tiles)
		{
			if(Branch.OwnerId == 0)
				continue;

			Tiles.Add(new SavedTile(Branch.ItemId, Branch.Translation, Branch.RotationDegrees));
		}

		S = Tiles.ToArray();
	}

	public SavedChunk()
	{}
}
