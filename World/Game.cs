using Godot;
using System;


public class Game : Node
{
	private static Node SteelGame;

	public static int MaxPlayers = 8;
	public static bool MouseLocked = false;
	public static bool BindsEnabled = true;
	public static System.Collections.Generic.Dictionary<int, Spatial> PlayerList = new System.Collections.Generic.Dictionary<int, Spatial>();
	public static Player PossessedPlayer = ((PackedScene)GD.Load("res://World/Player.tscn")).Instance() as Player;
	                                       //Prevent crashes when player movement commands are run when world is not initalized


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
		Player Player = ((PackedScene)GD.Load("res://World/Player.tscn")).Instance() as Player;
		Player.Possessed = Possess;
		Player.SetName(Id.ToString());
		PossessedPlayer = Player;
		PlayerList.Add(Id, (Spatial)Player);
		SteelGame.GetNode("SkyScene").AddChild(Player);
	}


	public static void CloseWorld()
	{
		if(SteelGame.HasNode("SkyScene"))
		{
			SteelGame.GetNode("SkyScene").QueueFree();
		}
	}


	public static void StartWorld()
	{
		CloseWorld();
		Node World = ((PackedScene)GD.Load("res://scenes/SkyScene.tscn")).Instance();
		World.SetName("SkyScene");
		SteelGame.AddChild(World);

		for(int X = 0; X <= 10; X++)
		{
			for(int Y = 0; Y <= 10; Y++)
			{
				Spatial Platform = (Spatial)((PackedScene)GD.Load("res://scenes/structures/Platform.tscn")).Instance();
				Platform.Translate(new Vector3(X*12,0,Y*12));
				World.AddChild(Platform);
			}
		}
	}

}
