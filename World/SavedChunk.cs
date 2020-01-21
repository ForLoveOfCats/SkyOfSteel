using Godot;
using System;


public class SavedChunk
{
	public int[] P;
	public SavedTile[] S; //Only used when deserializing

	Tuple<int,int> ChunkTuple;

	public SavedChunk(Tuple<int,int> ChunkTupleArg)
	{
		ChunkTuple = ChunkTupleArg;
		P = new int[2] {ChunkTuple.Item1, ChunkTuple.Item2};
	}

	public SavedChunk()
	{
	}


	public string ToJson()
	{
		string Out = $"{{\"P\":[{string.Join(",", P)}],\"S\":[";

		System.Collections.Generic.List<Tile> Tiles = World.Chunks[ChunkTuple].Tiles;
		foreach(Tile Branch in Tiles)
		{
			if(Branch.OwnerId == 0)
			{
				continue;
			}

			Out += new SavedTile(Branch.ItemId, Branch.Translation, Branch.RotationDegrees).ToJson() + ",";
		}

		if(Out[Out.Length-1] == ',')
		{
			Out = Out.Remove(Out.Length-1);
		}
		Out += "]}";

		return Out;
	}
}
