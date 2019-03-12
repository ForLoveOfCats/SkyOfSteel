using Godot;
using System;
using System.Collections.Generic;


public class World : Node
{
	public const int PlatformSize = 12;
	public const int ChunkSize = 9*PlatformSize;

	public static Dictionary<Items.TYPE, PackedScene> Scenes = new Dictionary<Items.TYPE, PackedScene>();

	public static Dictionary<Tuple<int,int>, ChunkClass> Chunks = new Dictionary<Tuple<int,int>, ChunkClass>();
	public static Dictionary<int, List<Tuple<int,int>>> RemoteLoadedChunks = new Dictionary<int, List<Tuple<int,int>>>();
	public static GridClass Grid = new GridClass();

	public static List<DroppedItem> DroppedItems = new List<DroppedItem>();

	private static PackedScene DroppedItemScene;

	public static World Self;

	World()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		DroppedItemScene = GD.Load<PackedScene>("res://Items/DroppedItem.tscn");

		Directory StructureDir = new Directory();
		StructureDir.Open("res://World/Scenes/");
		StructureDir.ListDirBegin(true, true);
		string FileName = StructureDir.GetNext();
		while(true)
		{
			if(FileName == "")
			{
				break;
			}
			PackedScene Scene = GD.Load("res://World/Scenes/"+FileName) as PackedScene;
			if((Scene.Instance() as Structure) == null)
			{
				throw new System.Exception("Structure scene '" + FileName + "' does not inherit Structure");
			}

			FileName = StructureDir.GetNext();
		}

		foreach(Items.TYPE Type in System.Enum.GetValues(typeof(Items.TYPE)))
		{
			File ToLoad = new File();
			if(ToLoad.FileExists("res://World/Scenes/" + Type.ToString() + ".tscn"))
			{
				Scenes.Add(Type, GD.Load("res://World/Scenes/" + Type.ToString() + ".tscn") as PackedScene);
			}
			else
			{
				Scenes.Add(Type, GD.Load("res://World/Scenes/ERROR.tscn") as PackedScene);
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


	static void AddStructureToChunk(Structure Branch)
	{
		if(ChunkExists(Branch.Translation))
		{
			List<Structure> Chunk = Chunks[GetChunkTuple(Branch.Translation)].Structures;
			Chunk.Add(Branch);
			Chunks[GetChunkTuple(Branch.Translation)].Structures = Chunk;
		}
		else
		{
			ChunkClass Chunk = new ChunkClass();
			Chunk.Structures = new List<Structure>{Branch};
			Chunks.Add(GetChunkTuple(Branch.Translation), Chunk);
		}
	}


	public static void AddItemToChunk(DroppedItem Item)
	{
		if(ChunkExists(Item.Translation))
		{
			List<DroppedItem> Items = Chunks[GetChunkTuple(Item.Translation)].Items;
			Items.Add(Item);
			Chunks[GetChunkTuple(Item.Translation)].Items = Items;
		}
		else
		{
			ChunkClass Chunk = new ChunkClass();
			Chunk.Items = new List<DroppedItem>{Item};
			Chunks.Add(GetChunkTuple(Item.Translation), Chunk);
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
			Net.SteelRpc(Self, nameof(PlaceWithName), new object[] {BranchType, Position, Rotation, OwnerId, Name});
		}
	}


	[Remote]
	public void InitialNetWorldLoad(int Id, Vector3 PlayerPosition, int RenderDistance)
	{
		RequestChunks(Id, PlayerPosition, RenderDistance);
		((Player)Game.RuntimeRoot.GetNode("SkyScene").GetNode(Id.ToString())).SetFreeze(false); //I hate casting syntax
	}


	[Remote]
	public void RequestChunks(int Id, Vector3 PlayerPosition, int RenderDistance) //Can be called non-rpc by passing int id
	{
		if(!GetTree().IsNetworkServer())
		{
			RpcId(Net.ServerId, nameof(RequestChunks), new object[] {Id, PlayerPosition, RenderDistance});
			return; //If not already on the server run on server and return early on client
		}

		if(!Net.PeerList.Contains(Id)) {return;}

		List<Tuple<int,int>> LoadedChunks = RemoteLoadedChunks[Id];
		foreach(KeyValuePair<System.Tuple<int, int>, ChunkClass> Chunk in Chunks)
		{
			Vector3 ChunkPos = new Vector3(Chunk.Key.Item1, 0, Chunk.Key.Item2);
			Tuple<int,int> ChunkTuple = GetChunkTuple(ChunkPos);
			if(ChunkPos.DistanceTo(new Vector3(PlayerPosition.x,0,PlayerPosition.z)) <= RenderDistance*(World.PlatformSize*9))
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
		World.Self.RpcId(Id, nameof(FreeChunk), new Vector2(ChunkLocation.Item1, ChunkLocation.Item2));
		foreach(Structure Branch in Chunks[ChunkLocation].Structures)
		{
			World.Self.RpcId(Id, nameof(World.PlaceWithName), new object[] {Branch.Type, Branch.Translation, Branch.RotationDegrees, Branch.OwnerId, Branch.GetName()});
		}
	}


	public static int SaveChunk(Tuple<int,int> ChunkTuple, string SaveName)
	{
		string SerializedChunk = new SavedChunk(ChunkTuple).ToJson();

		Directory SaveDir = new Directory();
		if(!SaveDir.DirExists("user://saves/"+SaveName))
		{
			SaveDir.MakeDirRecursive("user://saves/"+SaveName);
		}
		System.IO.File.WriteAllText(OS.GetUserDataDir() + "/saves/" + SaveName + "/" + ChunkTuple.ToString() + ".json", SerializedChunk);

		int SaveCount = 0;
		foreach(Structure Branch in Chunks[ChunkTuple].Structures) //I hate to do this because it is rather inefficient
		{
			if(Branch.OwnerId != 0)
			{
				SaveCount += 1;
			}
		}
		return SaveCount;
	}


	[Remote]
	public void FreeChunk(Vector2 Pos)
	{
		ChunkClass Branches;
		if(Chunks.TryGetValue(new Tuple<int,int>((int)Pos.x, (int)Pos.y), out Branches))
		{
			foreach(Structure Branch in Branches.Structures)
			{
				Branch.Free();
			}
		}
	}


	[Remote]
	public void PlaceWithName(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId, string Name)
	{
		Vector3 LevelPlayerPos = new Vector3(Game.PossessedPlayer.Translation.x,0,Game.PossessedPlayer.Translation.z);

		//Nested if to prevent very long line
		if(GetTree().NetworkPeer != null && !GetTree().IsNetworkServer())
		{
			if(GetChunkPos(Position).DistanceTo(LevelPlayerPos) > Game.ChunkRenderDistance*(World.PlatformSize*9))
			{
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

			AddStructureToChunk(Branch);
			Grid.AddItem(Branch);

			//Nested if to prevent very long line
			if(GetTree().NetworkPeer != null && GetTree().IsNetworkServer())
			{
				if(GetChunkPos(Position).DistanceTo(LevelPlayerPos) > Game.ChunkRenderDistance*(World.PlatformSize*9))
				{
					//If network is inited, am the server, and platform is to far away then...
					Branch.Hide(); //...make it not visible but allow it to remain in the world
				}
			}
		}
	}


	//Name is the string GUID name of the structure to be removed
	[Remote]
	public void Remove(string Name)
	{
		if(Game.StructureRoot.HasNode(Name))
		{
			Structure Branch = Game.StructureRoot.GetNode(Name) as Structure;
			Tuple<int,int> ChunkTuple = World.GetChunkTuple(Branch.Translation);
			List<Structure> Structures = World.Chunks[ChunkTuple].Structures;
			Structures.Remove(Branch);
			//After removing `this` from the Structure list, the chunk might be empty
			if(Structures.Count > 0)
			{
				World.Chunks[ChunkTuple].Structures = Structures;
			}
			else
			{
				//If the chunk *is* empty then remove it
				World.Chunks.Remove(ChunkTuple);
			}

			World.Grid.QueueUpdateNearby(Branch.Translation);
			World.Grid.QueueRemoveItem(Branch);
			Branch.QueueFree();
		}

	}


	//Should be able to be called without RPC yet only run on server
	//Has to be non-static to be RPC-ed
	[Remote]
	public void DropItem(Items.TYPE Type, Vector3 Position, Vector3 BaseMomentum)
	{
		if(Self.GetTree().GetNetworkPeer() != null)
		{
			if(Self.GetTree().IsNetworkServer())
			{
				Net.SteelRpc(Self, nameof(NetDropItem), Type, Position, BaseMomentum);
				NetDropItem(Type, Position, BaseMomentum);
			}
			else
			{
				Self.RpcId(Net.ServerId, nameof(DropItem), Type, Position, BaseMomentum);
			}
		}
	}


	//Has to be non-static to be RPC-ed
	[Remote]
	public void NetDropItem(Items.TYPE Type, Vector3 Position, Vector3 BaseMomentum)
	{
		DroppedItem ToDrop = DroppedItemScene.Instance() as DroppedItem;
		ToDrop.Translation = Position;
		ToDrop.Momentum = BaseMomentum;
		ToDrop.Type = Type;
		ToDrop.GetNode<MeshInstance>("MeshInstance").Mesh = Items.Meshes[Type];

		DroppedItems.Add(ToDrop);
		AddItemToChunk(ToDrop);
		Game.StructureRoot.AddChild(ToDrop);
	}


	public override void _Process(float Delta)
	{
		Grid.DoWork();
	}
}
