using Godot;
using System;
using System.Collections.Generic;


public class Game : Node
{
	public static Node RuntimeRoot;

	public static int MaxPlayers = 8;
	public static bool BindsEnabled = false;
	public static System.Collections.Generic.Dictionary<int, Spatial> PlayerList = new System.Collections.Generic.Dictionary<int, Spatial>();
	public static Player PossessedPlayer = ((PackedScene)GD.Load("res://Player/Player.tscn")).Instance() as Player;
										   //Prevent crashes when player movement commands are run when world is not initalized

	public static StructureRootClass StructureRoot;

	public static float MouseSensitivity = 1;
	public static int ChunkRenderDistance = 1;

	public static Game Self;
	private Game()
	{
		Self = this;
	}


	public override void _Ready()
	{
		RuntimeRoot = GetTree().GetRoot().GetNode("RuntimeRoot");
		GetTree().SetAutoAcceptQuit(false);

		Menu.Setup();
		Menu.BuildIntro();
	}


	public override void _Notification(int What)
	{
		if(What == MainLoop.NotificationWmQuitRequest)
		{
			Game.Quit();
		}
	}


	public override void _Process(float Delta)
	{
		if(Input.IsActionJustPressed("ui_cancel"))
		{
			Game.Quit();
		}

		if(Input.IsActionJustPressed("ConsoleToggle"))
		{
			if(Console.IsOpen)
			{
				Console.Close();
			}
			else
			{
				Console.Open();
			}
		}
	}


	public static void Quit()
	{
		CloseWorld();
		Self.GetTree().SetNetworkPeer(null);
		Self.GetTree().Quit();
	}


	public static void SpawnPlayer(int Id, bool Possess)
	{
		Player Player = ((PackedScene)GD.Load("res://Player/Player.tscn")).Instance() as Player;
		Player.Possessed = Possess;
		Player.Id = Id;
		Player.SetName(Id.ToString());
		PlayerList.Add(Id, (Spatial)Player);
		RuntimeRoot.GetNode("SkyScene").AddChild(Player);

		if(Possess)
		{
			PossessedPlayer = Player;
		}
	}


	public static void StartWorld(bool AsServer = false)
	{
		CloseWorld();
		Node SkyScene = ((PackedScene)GD.Load("res://World/SkyScene.tscn")).Instance();
		SkyScene.SetName("SkyScene");
		RuntimeRoot.AddChild(SkyScene);

		StructureRoot = new StructureRootClass();
		StructureRoot.SetName("StructureRoot");
		SkyScene.AddChild(StructureRoot);

		if(AsServer)
		{
			Scripting.SetupServerEngine();
			Building.Place(Items.TYPE.PLATFORM, new Vector3(), new Vector3(), 0);
		}

		Menu.Close();
	}


	public static void CloseWorld()
	{
		if(RuntimeRoot.HasNode("SkyScene"))
		{
			RuntimeRoot.GetNode("SkyScene").Free();
			//Free instead of QueueFree to prevent crash when starting new world in same frame
		}
		PlayerList.Clear();
		PossessedPlayer = ((PackedScene)GD.Load("res://Player/Player.tscn")).Instance() as Player;
						  //Prevent crashes when player movement commands are run when world is not initalized
		StructureRoot = null;
		Scripting.GamemodeName = null;
		Scripting.SetupServerEngine();
		Scripting.SetupClientEngine();
		Scripting.ClientGmScript = null;

		Building.Chunks.Clear();
		Building.RemoteLoadedChunks.Clear();
		Building.Grid.Clear();

		Console.ClearLog();
		Console.Log("");
	}


	public static void SaveWorld(string SaveName)
	{
		Directory SaveDir = new Directory();
		if(SaveDir.DirExists("user://saves/" + SaveName))
		{
			System.IO.Directory.Delete(OS.GetUserDataDir() + "/saves/" + SaveName, true);
		}

		int SaveCount = 0;
		foreach(KeyValuePair<System.Tuple<int, int>, List<Structure>> Chunk in Building.Chunks)
		{
			SaveCount += Building.SaveChunk(Chunk.Key, SaveName);
		}
		Console.Log($"Saved {SaveCount.ToString()} structures to save '{SaveName}'");
	}


	public static bool LoadWorld(string SaveName)
	{
		Directory SaveDir = new Directory();
		if(SaveDir.DirExists("user://saves/"+SaveName))
		{
			List<Structure> Branches = new List<Structure>();
			foreach(KeyValuePair<Tuple<int,int>, List<Structure>> Chunk in Building.Chunks)
			{
				foreach(Structure Branch in Chunk.Value)
				{
					Branches.Add(Branch);
				}
			}
			foreach(Structure Branch in Branches)
			{
				Branch.Remove();
			}
			Building.Chunks.Clear();
			Building.RemoteLoadedChunks.Clear();
			Building.Grid.Clear();

			SaveDir.Open("user://saves/"+SaveName);
			SaveDir.ListDirBegin(true, true);

			int PlaceCount = 0;
			while(true)
			{
				string FileName = SaveDir.GetNext();
				if(FileName.Empty())
				{
					//Iterated through all files
					break;
				}

				string LoadedFile = System.IO.File.ReadAllText($"{OS.GetUserDataDir()}/saves/{SaveName}/{FileName}");

				SavedChunk LoadedChunk;
				try
				{
					LoadedChunk = Newtonsoft.Json.JsonConvert.DeserializeObject<SavedChunk>(LoadedFile);
				}
				catch(Newtonsoft.Json.JsonReaderException)
				{
					Console.ThrowLog($"Invalid chunk file {FileName} loading save '{SaveName}'");
					continue;
				}

				foreach(SavedStructure SavedBranch in LoadedChunk.S)
				{
					Tuple<Items.TYPE,Vector3,Vector3> Info = SavedBranch.GetInfoOrNull();
					if(Info != null)
					{
						Building.Place(Info.Item1, Info.Item2, Info.Item3, 0);
						PlaceCount++;
					}
				}
			}
			Console.Log($"Loaded {PlaceCount.ToString()} structures from save '{SaveName}'");
			return true;
		}
		else
		{
			Console.ThrowLog($"Failed to load save '{SaveName}' as it does not exist");
			return false;
		}
	}
}
