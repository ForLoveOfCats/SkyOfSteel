using Godot;
using System;


public class Game : Node
{
	public const string Version = "0.1.4"; //Yes it's a string shush
	public static string RemoteVersion = null; //What is the latest version available online
	public const string DefaultNickname = "BrianD";

	public static Node RuntimeRoot;

	public static int MaxPlayers = 8;
	public static bool BindsEnabled = false;
	public static Player PossessedPlayer;

	public static Gamemode Mode = new Gamemode(); //Get it? Game.Mode Mwa ha ha ha

	public static float LookSensitivity = 15;
	public static float MouseDivisor = LookSensitivity;
	public static float Deadzone = 0.25f;
	public static int ChunkRenderDistance = 1;

	public static string Nickname = DefaultNickname;

	public static Game Self;
	private Game()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		using(System.Net.WebClient WebClient = new System.Net.WebClient())
		{
			try //Make sure game is still playable if url becomes invalid
			{
				RemoteVersion = WebClient.DownloadString("http://skyofsteel.org/LatestVersion.txt");
				RemoteVersion = RemoteVersion.Trim();
			}
			catch
			{}
		}
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

		if(Input.IsActionJustPressed("ConsoleToggle") && !Menu.PauseOpen)
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

		if(Possess)
		{
			PossessedPlayer = NewPlayer;
		}

		RuntimeRoot.GetNode("SkyScene").AddChild(NewPlayer);

		NewPlayer.MovementReset();
	}


	static public void CopyFolder(string SourceFolder, string DestFolder) //TODO: Clean up
	{
		if(System.IO.Directory.Exists(DestFolder))
			System.IO.Directory.CreateDirectory(DestFolder);

		string[] Files = System.IO.Directory.GetFiles(SourceFolder);
		foreach(string File in Files)
		{
			string Name = System.IO.Path.GetFileName(File);
			string Dest = System.IO.Path.Combine(DestFolder, Name);
			System.IO.File.Copy(File, Dest);
		}

		string[] Folders = System.IO.Directory.GetDirectories(SourceFolder);
		foreach(string Folder in Folders)
		{
			string Name = System.IO.Path.GetFileName(Folder);
			string Dest = System.IO.Path.Combine(DestFolder, Name);
			CopyFolder(Folder, Dest);
		}
	}
}
