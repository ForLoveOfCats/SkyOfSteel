using Godot;
using System;


public class Game : Node
{
	public static Node RuntimeRoot;

	public static int MaxPlayers = 8;
	public static bool MouseLocked = false;
	public static bool BindsEnabled = true;
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
		Input.SetMouseMode(Input.MouseMode.Captured);
		MouseLocked = true;
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

		if(Input.IsActionJustPressed("MouseLock"))
		{
			if(Input.GetMouseMode() == 0)
			{
				Input.SetMouseMode(Input.MouseMode.Captured);
				MouseLocked = true;
				BindsEnabled = true;
				((ConsoleWindow)RuntimeRoot.GetNode("ConsoleWindow")).WindowVisible(false);
			}

			else
			{
				Input.SetMouseMode(Input.MouseMode.Visible);
				MouseLocked = false;
				BindsEnabled = false;
				((ConsoleWindow)RuntimeRoot.GetNode("ConsoleWindow")).WindowVisible(true);
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
	}

}
