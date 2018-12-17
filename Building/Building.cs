using Godot;
using System;


public class Building : Node
{
	public const int ChunkSize = 9*12; //every 12 units is a new platform and the chunk size is 9x9 platforms

	private static Dictionary<Items.TYPE, PackedScene> Scenes = new Dictionary<Items.TYPE, PackedScene>();

	public static Building Self;

	Building()
	{
		Self = this;

		Directory StructureDir = new Directory();
		StructureDir.Open("res://Building/Scenes/");
		StructureDir.ListDirBegin(true, true);
		string FileName = StructureDir.GetNext();
		while(true)
		{
			if(FileName == "")
			{
				break;
			}
			PackedScene Scene = GD.Load("res://Building/Scenes/"+FileName) as PackedScene;
			if((Scene.Instance() as Structure) == null)
			{
				throw new System.Exception("Structure scene '" + FileName + "' does not inherit Structure");
			}

			FileName = StructureDir.GetNext();
		}

		foreach(Items.TYPE Type in System.Enum.GetValues(typeof(Items.TYPE)))
		{
			File ToLoad = new File();
			if(ToLoad.FileExists("res://Building/Scenes/" + Type.ToString() + ".tscn"))
			{
				Scenes.Add(Type, GD.Load("res://Building/Scenes/" + Type.ToString() + ".tscn") as PackedScene);
			}
			else
			{
				Scenes.Add(Type, GD.Load("res://Building/Scenes/ERROR.tscn") as PackedScene);
			}
		}
	}


	static Tuple<int,int> GetChunkPos(Vector3 Position)
	{
		return new Tuple<int,int>(Mathf.RoundToInt(Position.x/ChunkSize)*ChunkSize, Mathf.RoundToInt(Position.z/ChunkSize)*ChunkSize);
	}

	
	static bool ChunkExists(Vector3 Position)
	{
		return Game.StructureRoot.Chunks.ContainsKey(GetChunkPos(Position));
	}

	static System.Collections.Generic.List<Structure> GetChunk(Vector3 Position)
	{
		if(ChunkExists(Position))
		{
			return Game.StructureRoot.Chunks[GetChunkPos(Position)];
		}
		return null; //uggggh whyyyyyyyy
	}

	static void AddToChunk(Structure Branch)
	{		
		if(ChunkExists(Branch.Translation))
		{
			System.Collections.Generic.List<Structure> Chunk = Game.StructureRoot.Chunks[GetChunkPos(Branch.Translation)];
			Chunk.Add(Branch);
			Game.StructureRoot.Chunks[GetChunkPos(Branch.Translation)] = Chunk;
		}
		else
		{
			Game.StructureRoot.Chunks.Add(GetChunkPos(Branch.Translation), new System.Collections.Generic.List<Structure>{Branch});
		}
	}


	public static void PlaceOn(Structure Base, Items.TYPE BranchType, int OwnerId)
	{
		System.Nullable<Vector3> Position = BuildPositions.Calculate(Base, BranchType);
		if(Position != null) //If null then unsupported branch/base combination
		{
			Vector3 Rotation = BuildRotations.Calculate(Base, BranchType);
			Place(BranchType, (Vector3)Position, Rotation, OwnerId);
		}
	}


	public static void Place(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId)
	{
		string Name = System.Guid.NewGuid().ToString();
		Self.PlaceWithName(BranchType, Position, Rotation, OwnerId, Name);

		if(Self.GetTree().NetworkPeer != null) //Don't sync place if network is not ready
		{
			Self.Rpc(nameof(PlaceWithName), new object[] {BranchType, Position, Rotation, OwnerId, Name});
		}
	}


	[Remote]
	public void PlaceWithName(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId, string Name)
	{
		if(ShouldDo.StructurePlace(BranchType, Position, Rotation, OwnerId))
		{
			Structure Branch = Scenes[BranchType].Instance() as Structure;
			Branch.Type = BranchType;
			Branch.OwnerId = OwnerId;
			Branch.Translation = Position;
			Branch.RotationDegrees = Rotation;
			Branch.SetName(Name); //Name is a GUID and can be used to reference a structure over network
			Game.StructureRoot.AddChild(Branch);

			AddToChunk(Branch);
		}
	}


	//Name is the string GUID name of the structure to be removed
	public static void Remove(string Name)
	{
		Structure Branch = Game.StructureRoot.GetNode(Name) as Structure;
		Branch.Remove();
	}
}
