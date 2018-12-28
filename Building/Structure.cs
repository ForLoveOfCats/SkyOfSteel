using Godot;


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
		System.Collections.Generic.List<Structure> Structures = Building.Chunks[Building.GetChunkTuple(Translation)];
		Structures.Remove(this);
		Building.Chunks[Building.GetChunkTuple(Translation)] = Structures;
		QueueFree();
	}
}
