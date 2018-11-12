using Godot;
using System;


public class Game : Node
{
	private static Node SteelGame;

	public static int MaxPlayers = 8;
	public static bool MouseLocked = false;
	public static bool BindsEnabled = true;
	public static System.Collections.Generic.Dictionary<int, Spatial> PlayerList = new System.Collections.Generic.Dictionary<int, Spatial>();
	public static Player PossessedPlayer = ((PackedScene)GD.Load("res://World/Player/Player.tscn")).Instance() as Player;
	                                       //Prevent crashes when player movement commands are run when world is not initalized

	public static Node StructureRoot;

	public static float MouseSensitivity = 1;

	private static Game Self;
	private Game()
	{
		Self = this;
	}


	public override void _Ready()
	{
		SteelGame = GetTree().GetRoot().GetNode("SteelGame");
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
				SteelGame.GetNode("ConsoleWindow").Call("hide");
			}

			else
			{
				Input.SetMouseMode(Input.MouseMode.Visible);
				MouseLocked = false;
				BindsEnabled = false;
				SteelGame.GetNode("ConsoleWindow").Call("show");
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
		Player Player = ((PackedScene)GD.Load("res://World/Player/Player.tscn")).Instance() as Player;
		Player.Possessed = Possess;
		Player.SetName(Id.ToString());
		PlayerList.Add(Id, (Spatial)Player);
		SteelGame.GetNode("SkyScene").AddChild(Player);

		if(Possess)
		{
			PossessedPlayer = Player;
		}
	}


	public static void CloseWorld()
	{
		if(SteelGame.HasNode("SkyScene"))
		{
			SteelGame.GetNode("SkyScene").QueueFree();
		}
		PossessedPlayer = ((PackedScene)GD.Load("res://World/Player/Player.tscn")).Instance() as Player;
		                  //Prevent crashes when player movement commands are run when world is not initalized
		StructureRoot = null;
	}


	public static void StartWorld()
	{
		CloseWorld();
		Node SkyScene = ((PackedScene)GD.Load("res://scenes/SkyScene.tscn")).Instance();
		SkyScene.SetName("SkyScene");
		SteelGame.AddChild(SkyScene);

		StructureRoot = new Node();
		StructureRoot.SetName("StructureRoot");
		SkyScene.AddChild(StructureRoot);

		for(int X = 0; X <= 10; X++)
		{
			for(int Z = 0; Z <= 10; Z++)
			{
				Structures.Place(Items.TYPE.PLATFORM, new Vector3(X*12,0,Z*12), new Vector3(), 0);
			}
		}
	}

}
