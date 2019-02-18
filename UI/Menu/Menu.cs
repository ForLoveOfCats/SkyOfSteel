using Godot;


public class Menu : Node
{
	public static bool IsOpen = true;
	public static bool PauseOpen = false;

	private static ScrollContainer Center;
	private static Node Contents = null;

	private static PackedScene Intro;
	private static PackedScene Main;
	private static PackedScene Host;
	private static PackedScene SavedHost;
	private static PackedScene ConnectMenu;
	private static PackedScene PauseMenu;

	static Menu()
	{
		if(Engine.EditorHint) {return;}

		//All menu scene files are loaded on game startup
		Intro = GD.Load<PackedScene>("res://UI/Menu/Intro/Intro.tscn");
		Main = GD.Load<PackedScene>("res://UI/Menu/MainMenu/MainMenu.tscn");
		Host = GD.Load<PackedScene>("res://UI/Menu/HostMenu/HostMenu.tscn");
		SavedHost = GD.Load<PackedScene>("res://UI/Menu/SavedWorldHost/SavedWorldHost.tscn");
		ConnectMenu = GD.Load<PackedScene>("res://UI/Menu/ConnectMenu/ConnectMenu.tscn");
		PauseMenu = GD.Load<PackedScene>("res://UI/Menu/PauseMenu/PauseMenu.tscn");
	}

	public static void Setup() //Called from Game.cs before this class's _Ready would
	{
		Center = Game.RuntimeRoot.GetNode("MenuRoot").GetNode("HBox/Center") as ScrollContainer;
	}


	public static void Reset()
	{
		if(Contents != null)
		{
			Contents.QueueFree();
			Contents = null;
		}

		IsOpen = true;
		Game.BindsEnabled = false;
		Input.SetMouseMode(Input.MouseMode.Visible);
	}


	public static void Close()
	{
		Reset();
		IsOpen = false;
		PauseOpen = false;

		if(!Console.IsOpen)
		{
			Game.BindsEnabled = true;
			Input.SetMouseMode(Input.MouseMode.Captured);
		}
	}


	public static void BuildIntro()
	{
		Reset();

		Contents = Intro.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildMain()
	{
		Reset();

		Contents = Main.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildHost()
	{
		Reset();

		Contents = Host.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildSavedHost()
	{
		Reset();

		Contents = SavedHost.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildConnect()
	{
		Reset();

		Contents = ConnectMenu.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildPause()
	{
		Reset();

		Contents = PauseMenu.Instance();
		Center.AddChild(Contents);
		PauseOpen = true;
	}
}
