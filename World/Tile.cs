using Godot;
using System;
using Optional;



public class SavedTile
{
	public int I = (int)Items.ID.ERROR;
	public int O;
	public Vector3 P;
	public Vector3 R;
	[Newtonsoft.Json.JsonProperty("V")]
	public int InventoryIndex = -1;

	public SavedTile(SavedChunk Chunk, Tile Branch)
	{
		I = (int)Branch.ItemId;
		O = Branch.OwnerId;
		P = Branch.Translation;
		R = Branch.RotationDegrees;

		if(Branch is IHasInventory HasInventory)
			InventoryIndex = Chunk.AddInventory(new SavedInventory(HasInventory.Inventory));

		//TODO: Hmmmmm
		for(int i = 0; i <= 2; i++)
		{
			P[i] = (float)Math.Round(P[i]);
			R[i] = (float)Math.Round(R[i]);
		}
	}

	public SavedTile()
	{}
}



public class Tile : StaticBody, IEntity, IInGrid
{
	public System.Tuple<int, int> CurrentChunk { get; set; }
	public Items.ID ItemId = Items.ID.ERROR;
	public int OwnerId = 0;
	public Pathfinding.PointData Point = null;


	public override void _Ready()
	{
		World.AddEntityToChunk(this);
	}


	public override void _ExitTree()
	{
		World.RemoveEntityFromChunk(this);
	}


	public virtual void GridUpdate()
	{}


	[Remote]
	public void PhaseOut()
	{
		World.Self.RemoveTile(Name);
	}


	[Remote]
	public void Destroy(params object[] Args)
	{
		Assert.ArgArray(Args);

		if(OwnerId != 0)
			World.Self.RemoveTile(Name);
	}


	[Remote]
	public virtual void Update(params object[] Args)
	{
		Assert.ArgArray(Args);
	}


	public virtual void OnRemove()
	{}


	[Remote]
	public void NetRemove()
	{
		if(Net.Work.IsNetworkServer())
		{
			Entities.SendDestroy(Name);
			Destroy();
		}
		else
			Entities.Self.PleaseDestroyMe(Name);
	}


	public static Option<Tile> None()
	{
		return Option.None<Tile>();
	}


	public Option<Tile> Some()
	{
		return Option.Some(this);
	}
}
