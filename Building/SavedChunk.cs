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

		System.Collections.Generic.List<Structure> Structures = Building.GetChunk(ChunkTuple);
		foreach(Structure Branch in Structures)
		{
			Out += new SavedStructure(Branch.Type, Branch.Translation, Branch.RotationDegrees).ToJson() + ",";
		}
		Out = Out.Remove(Out.Length-1);
		Out += "]}";

		return Out;
	}
}
