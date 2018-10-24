using Godot;
using System;


public class Game : Node
{
	private static Node SteelGame;

	public static int MaxPlayers = 8;
	public static bool MouseLocked = false;
	public static bool PlayerInputEnabled = true;
	public static System.Collections.Generic.Dictionary<int, Spatial> PlayerList = new System.Collections.Generic.Dictionary<int, Spatial>();


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
				PlayerInputEnabled = true;
				SteelGame.GetNode("ConsoleWindow").Call("hide");
			}

			else
			{
				Input.SetMouseMode(Input.MouseMode.Visible);
				MouseLocked = false;
				PlayerInputEnabled = false;
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
		Node Player = ((PackedScene)GD.Load("res://scenes/Player.tscn")).Instance();
		Player.Set("possessed", Possess);
		Player.SetName(Id.ToString());
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
