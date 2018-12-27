using Godot;
using System;
using Collections = System.Collections.Generic;


public class Building : Node
{
	public const int PlatformSize = 12;
	public const int ChunkSize = 9*PlatformSize;

	private static Dictionary<Items.TYPE, PackedScene> Scenes = new Dictionary<Items.TYPE, PackedScene>();

	public static Collections.Dictionary<Tuple<int,int>, Collections.List<Structure>> Chunks = new Collections.Dictionary<Tuple<int,int>, Collections.List<Structure>>();
	public static Collections.Dictionary<int, Collections.List<Tuple<int,int>>> RemoteLoadedChunks = new Collections.Dictionary<int, Collections.List<Tuple<int,int>>>();

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


	public static Vector3 GetChunkPos(Vector3 Position)
	{
		return new Vector3(Mathf.RoundToInt(Position.x/ChunkSize)*ChunkSize, 0, Mathf.RoundToInt(Position.z/ChunkSize)*ChunkSize);
	}


	public static Tuple<int,int> GetChunkTuple(Vector3 Position)
	{
		return new Tuple<int,int>(Mathf.RoundToInt(Position.x/ChunkSize)*ChunkSize, Mathf.RoundToInt(Position.z/ChunkSize)*ChunkSize);
	}

	
	static bool ChunkExists(Vector3 Position)
	{
		return ChunkExists(GetChunkTuple(Position));
	}


	static bool ChunkExists(Tuple<int, int> Position)
	{
		return Chunks.ContainsKey(Position);
	}


	static System.Collections.Generic.List<Structure> GetChunk(Vector3 Position)
	{
		return GetChunk(GetChunkTuple(Position));
	}


	static System.Collections.Generic.List<Structure> GetChunk(Tuple<int, int> Position)
	{
		if(ChunkExists(Position))
		{
			return Chunks[Position];
		}
		return null; //uggggh whyyyyyyyy
	}


	static void AddToChunk(Structure Branch)
	{		
		if(ChunkExists(Branch.Translation))
		{
			System.Collections.Generic.List<Structure> Chunk = Chunks[GetChunkTuple(Branch.Translation)];
			Chunk.Add(Branch);
			Chunks[GetChunkTuple(Branch.Translation)] = Chunk;
		}
		else
		{
			Chunks.Add(GetChunkTuple(Branch.Translation), new System.Collections.Generic.List<Structure>{Branch});
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
	public void RequestChunks(int Id, Vector3 PlayerPosition, int RenderDistance)
	{
		if(!GetTree().IsNetworkServer())
		{
			RpcId(Net.ServerId, nameof(RequestChunks), new object[] {Id, PlayerPosition, RenderDistance});
			return;
		}

		Collections.List<Tuple<int,int>> LoadedChunks = RemoteLoadedChunks[Id];
		foreach(Collections.KeyValuePair<System.Tuple<int, int>, Collections.List<Structure>> Chunk in Chunks)
		{
			Vector3 ChunkPos = new Vector3(Chunk.Key.Item1, 0, Chunk.Key.Item2);
			Tuple<int,int> ChunkTuple = GetChunkTuple(ChunkPos);
			if(ChunkPos.DistanceTo(PlayerPosition) <= RenderDistance*(Building.PlatformSize*9))
			{
				//This chunk is close enough to the player that we should send it along
				if(!LoadedChunks.Contains(ChunkTuple))
				{
					//If not already in the list of loaded chunks for this client then add it
					RemoteLoadedChunks[Id].Add(ChunkTuple);
					//And send it
					SendChunk(Id, ChunkTuple);
				}
				//If already loaded then don't send it
			}
			else
			{
				//This chunk is to far away
				if(LoadedChunks.Contains(ChunkTuple))
				{
					//If it is in the list of loaded chunks for this client then remove
					RemoteLoadedChunks[Id].Remove(ChunkTuple);
				}
			}
		}
	}


	static void SendChunk(int Id, Tuple<int,int> ChunkLocation)
	{
		foreach(Structure Branch in Chunks[ChunkLocation])
		{
			Building.Self.RpcId(Id, nameof(Building.PlaceWithName), new object[] {Branch.Type, Branch.Translation, Branch.RotationDegrees, Branch.OwnerId, Branch.GetName()});
		}
	}


	[Remote]
	public void PlaceWithName(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId, string Name)
	{
		//Nested if to prevent very long line
		if(GetTree().NetworkPeer != null && !GetTree().IsNetworkServer())
		{
			if(GetChunkPos(Position).DistanceTo(Game.PossessedPlayer.Translation) > Game.ChunkRenderDistance*(Building.PlatformSize*9))
			{
				GD.Print("PlaceWithName return");
				//If network is inited, not the server, and platform it to far away then...
				return; //...don't place
			}
		}

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

			//Nested if to prevent very long line
			if(GetTree().NetworkPeer != null && GetTree().IsNetworkServer())
			{
				if(GetChunkPos(Position).DistanceTo(Game.PossessedPlayer.Translation) > Game.ChunkRenderDistance*(Building.PlatformSize*9))
				{
					GD.Print("PlaceWithName hide");
					//If network is inited, are the server, and platform is to far away then...
					Branch.Hide(); //...make it not visible but allow it to remain in the world
				}
			}
		}
	}


	//Name is the string GUID name of the structure to be removed
	public static void Remove(string Name)
	{
		Structure Branch = Game.StructureRoot.GetNode(Name) as Structure;
		Branch.Remove();
	}
}
