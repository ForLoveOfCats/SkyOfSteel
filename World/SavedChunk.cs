using Godot;
using System;


public class SavedChunk
{
	public int[] P;
	public SavedStructure[] S; //Only used when deserializing

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

		System.Collections.Generic.List<Structure> Structures = World.Chunks[ChunkTuple].Structures;
		foreach(Structure Branch in Structures)
		{
			if(Branch.OwnerId == 0)
			{
				continue;
			}

			Out += new SavedStructure(Branch.Type, Branch.Translation, Branch.RotationDegrees).ToJson() + ",";
		}

		if(Out[Out.Length-1] == ',')
		{
			Out = Out.Remove(Out.Length-1);
		}
		Out += "]}";

		return Out;
	}
}
