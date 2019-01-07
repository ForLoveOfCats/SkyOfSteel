using Godot;
using System;


public class Structure : StaticBody
{
	public Items.TYPE Type = Items.TYPE.ERROR;
	public int OwnerId = 0;


	public void Remove()
	{
		if(ShouldDo.StructureRemove(Type, Translation, RotationDegrees, OwnerId))
		{
			Rpc(nameof(NetRemove));
			NetRemove();
		}
	}


	[Remote]
	public void NetRemove()
	{
		Tuple<int,int> ChunkTuple = Building.GetChunkTuple(Translation);
		System.Collections.Generic.List<Structure> Structures = Building.Chunks[ChunkTuple];
		Structures.Remove(this);
		//After removing `this` from the Structure list, the chunk might be empty
		if(Structures.Count > 0)
		{
			Building.Chunks[ChunkTuple] = Structures;
		}
		else
		{
			//If the chunk *is* empty then remove it
			Building.Chunks.Remove(ChunkTuple);
		}

		QueueFree();
	}
}
