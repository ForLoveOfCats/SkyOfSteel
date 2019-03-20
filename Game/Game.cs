using Godot;
using System;
using System.Collections.Generic;


public class Game : Node
{
	public const string Version = "0.1.2-dev"; //Yes it's a string shush

	public static Node RuntimeRoot;

	public static int MaxPlayers = 8;
	public static bool BindsEnabled = false;
	public static Dictionary<int, Spatial> PlayerList = new Dictionary<int, Spatial>();
	public static Player PossessedPlayer = GD.Load<PackedScene>("res://Player/Player.tscn").Instance() as Player;
										   //Prevent crashes when player movement commands are run when world is not initalized

	public static bool WorldOpen = false;
	public static StructureRootClass StructureRoot;

	public static float LookSensitivity = 5;
	public static float MouseDivisor = LookSensitivity;
	public static float Deadzone = 0.25f;
	public static int ChunkRenderDistance = 1;

	public static string Nickname = "";

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
			if(Console.IsOpen)
			{
				Console.Close();
			}
			else
			{
				if(Menu.PauseOpen)
				{
					Menu.Close();
				}
				else if(WorldOpen)
				{
					Menu.BuildPause();
				}
			}
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


	public override void _Input(InputEvent Event)
	{
		if(Event.IsAction("ConsoleToggle") && Input.IsActionJustPressed("ConsoleToggle"))
		{
			GetTree().SetInputAsHandled();
		}
	}


	public static void Quit()
	{
		Net.Disconnect();
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


	private static void SetupWorld()
	{
		World.Place(Items.TYPE.PLATFORM, new Vector3(), new Vector3(), 0);
	}


	public static void StartWorld(bool AsServer = false)
	{
		CloseWorld();
		Menu.Close();

		Node SkyScene = ((PackedScene)GD.Load("res://World/SkyScene.tscn")).Instance();
		SkyScene.SetName("SkyScene");
		RuntimeRoot.AddChild(SkyScene);

		StructureRoot = new StructureRootClass();
		StructureRoot.SetName("StructureRoot");
		SkyScene.AddChild(StructureRoot);

		Scripting.SetupGmEngine();

		if(AsServer)
		{
			SetupWorld();
		}

		WorldOpen = true;
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

		Scripting.UnloadGameMode();

		World.Chunks.Clear();
		World.RemoteLoadedChunks.Clear();
		World.Grid.Clear();

		WorldOpen = false;
	}


	public static void SaveWorld(string SaveName)
	{
		Directory SaveDir = new Directory();
		if(SaveDir.DirExists("user://saves/" + SaveName))
		{
			System.IO.Directory.Delete(OS.GetUserDataDir() + "/saves/" + SaveName, true);
		}

		int SaveCount = 0;
		foreach(KeyValuePair<System.Tuple<int, int>, ChunkClass> Chunk in World.Chunks)
		{
			SaveCount += World.SaveChunk(Chunk.Key, SaveName);
		}
		Console.Log($"Saved {SaveCount.ToString()} structures to save '{SaveName}'");
	}


	public static bool LoadWorld(string SaveName)
	{
		Directory SaveDir = new Directory();
		if(SaveDir.DirExists("user://saves/"+SaveName))
		{
			List<Structure> Branches = new List<Structure>();
			foreach(KeyValuePair<Tuple<int,int>, ChunkClass> Chunk in World.Chunks)
			{
				foreach(Structure Branch in Chunk.Value.Structures)
				{
					Branches.Add(Branch);
				}
			}
			foreach(Structure Branch in Branches)
			{
				Branch.Remove(Force:true);
			}
			World.Chunks.Clear();
			World.Grid.Clear();
			foreach(KeyValuePair<int, List<Tuple<int,int>>> Pair in World.RemoteLoadedChunks)
			{
				World.RemoteLoadedChunks[Pair.Key].Clear();
			}
			SetupWorld();

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
						World.Place(Info.Item1, Info.Item2, Info.Item3, 1);
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
