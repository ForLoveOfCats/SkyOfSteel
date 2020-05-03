using Godot;
using System;
using Optional;



public class Game : Node {
	public const string Version = "0.2-dev"; //Yes it's a string shush
	public static string RemoteVersion; //What is the latest version available online
	public const string DefaultNickname = "BrianD";

	public static Node RuntimeRoot;

	public static int MaxPlayers = 8;
	public static bool BindsEnabled = false;
	public static Option<Player> PossessedPlayer;

	public static float LookSensitivity = 15;
	public static float Deadzone = 0.25f;
	public static float Fov = 100;
	public static int ChunkRenderDistance = 1; //Actual default set by SetupDefaults

	public static string Nickname = DefaultNickname;

	public static Random Rand = new Random();

	public static Game Self;
	private Game() {
		if(Engine.EditorHint) { return; }

		Self = this;

		using System.Net.WebClient WebClient = new System.Net.WebClient();
		try //Make sure game is still playable if url becomes invalid
		{
			RemoteVersion = WebClient.DownloadString("http://skyofsteel.org/LatestVersion.txt");
			RemoteVersion = RemoteVersion.Trim();
		}
		catch { /*Ignored*/ }
	}


	public override void _Ready() {
		RuntimeRoot = GetTree().Root.GetNode("RuntimeRoot");
		GetTree().SetAutoAcceptQuit(false);

		Menu.Setup();

		if(OS.IsDebugBuild())
			Menu.BuildMain();
		else
			Menu.BuildIntro();
	}


	public override void _Notification(int What) {
		if(What == MainLoop.NotificationWmQuitRequest) {
			Game.Quit();
		}
	}


	public override void _Process(float Delta) {
		if(Input.IsActionJustPressed("ui_cancel")) {
			if(Console.IsOpen) {
				Console.Close();
			}
			else {
				if(Menu.IngameMenuOpen) {
					Menu.Close();
				}
				else if(World.IsOpen) {
					Menu.BuildPause();
				}
			}
		}

		if(Input.IsActionJustPressed("ConsoleToggle") && !Menu.IngameMenuOpen) {
			if(Console.IsOpen) {
				Console.Close();
			}
			else {
				Console.Open();
			}
		}
	}


	public override void _Input(InputEvent Event) {
		if(Event.IsAction("ConsoleToggle") && Input.IsActionJustPressed("ConsoleToggle")) {
			GetTree().SetInputAsHandled();
		}
	}


	public static void Quit() {
		Net.Disconnect();
		Self.GetTree().Quit();
	}


	[Remote]
	public void NetSpawnPlayer(int Id) {
		SpawnPlayer(Id, false);
	}


	public static void SpawnPlayer(int Id, bool Possess) {
		if(World.EntitiesRoot.HasNode(Id.ToString()))
			return;

		var NewPlayer = (Player)GD.Load<PackedScene>("res://Player/Player.tscn").Instance();
		NewPlayer.Possessed = Possess;
		NewPlayer.Id = Id;
		NewPlayer.Name = Id.ToString();
		Net.Players[Id].Plr = NewPlayer.Some();

		if(Possess)
			PossessedPlayer = NewPlayer.Some();

		World.EntitiesRoot.AddChild(NewPlayer);
		NewPlayer.MovementReset();
	}


	public static void CopyFolder(string SourceFolder, string DestFolder) //TODO: Clean up
	{
		if(System.IO.Directory.Exists(DestFolder))
			System.IO.Directory.CreateDirectory(DestFolder);

		string[] Files = System.IO.Directory.GetFiles(SourceFolder);
		foreach(string File in Files) {
			string Name = System.IO.Path.GetFileName(File);
			string Dest = System.IO.Path.Combine(DestFolder, Name);
			System.IO.File.Copy(File, Dest);
		}

		string[] Folders = System.IO.Directory.GetDirectories(SourceFolder);
		foreach(string Folder in Folders) {
			string Name = System.IO.Path.GetFileName(Folder);
			string Dest = System.IO.Path.Combine(DestFolder, Name);
			CopyFolder(Folder, Dest);
		}
	}
}
