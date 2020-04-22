using Godot;
using System;
using Optional;



public class SavedTile
{
	public int T = (int)Items.ID.ERROR;
	public float[] P;
	public float[] R;

	public SavedTile(Items.ID Type, Vector3 Position, Vector3 Rotation)
	{
		this.T = (int)Type;
		this.P = new float[3] {Position.x, Position.y, Position.z};
		this.R = new float[3] {Rotation.x, Rotation.y, Rotation.z};

		for(int i = 0; i <= 2; i++)
		{
			P[i] = (float)Math.Round(P[i]);
			R[i] = (float)Math.Round(R[i]);
		}
	}


	public Tuple<Items.ID,Vector3,Vector3> GetInfoOrNull()
	{
		//Returns null if data is invalid

		if(P.Length != 3)
		{
			return null;
		}
		Vector3 Pos = new Vector3(P[0], P[1], P[2]);

		if(R.Length != 3)
		{
			return null;
		}
		Vector3 Rot = new Vector3(R[0], R[1], R[2]);

		return new Tuple<Items.ID,Vector3,Vector3>((Items.ID)T, Pos, Rot);
	}
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


	public SavedTile ToSavable()
	{
		return new SavedTile(ItemId, Translation, RotationDegrees);
	}


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
