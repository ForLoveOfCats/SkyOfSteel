using Godot;
using System;


public class Game : Node
{
	public const string Version = "0.1.2-dev"; //Yes it's a string shush

	public static Node RuntimeRoot;

	public static int MaxPlayers = 8;
	public static bool BindsEnabled = false;
	public static Player PossessedPlayer;

	public static Gamemode Mode = new Gamemode(); //Get it? Game.Mode Mwa ha ha ha

	public static float LookSensitivity = 15;
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

		GetViewport().Msaa = Viewport.MSAA.Msaa4x; //Always on antialiasing, TODO add settings for this
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
				else if(World.IsOpen)
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
		Player NewPlayer = ((PackedScene)GD.Load("res://Player/Player.tscn")).Instance() as Player;
		NewPlayer.Possessed = Possess;
		NewPlayer.Id = Id;
		NewPlayer.SetName(Id.ToString());
		Net.Players.Add(Id, NewPlayer);
		RuntimeRoot.GetNode("SkyScene").AddChild(NewPlayer);

		if(Possess)
		{
			PossessedPlayer = NewPlayer;
		}
	}
}
