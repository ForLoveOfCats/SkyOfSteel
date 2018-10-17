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
	}


	public static void Set(string Object, bool Data)  //Temporary
	{
		switch(Object)
		{
			case "MouseLocked":
				MouseLocked = Data;
				break;
			case "PlayerInputEnabled":
				PlayerInputEnabled = Data;
				break;
		}
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
