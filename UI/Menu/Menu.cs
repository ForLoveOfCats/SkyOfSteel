using Godot;


public class Menu : Node
{
	public static bool IsOpen = true;
	public static bool IngameMenuOpen = false;

	private static Panel ShadedBackground;
	private static ScrollContainer Center;
	private static Node Contents = null;

	private static readonly PackedScene Intro;
	private static readonly PackedScene Update;
	private static readonly PackedScene Nick;
	private static readonly PackedScene Main;
	private static readonly PackedScene Help;
	private static readonly PackedScene Credits;
	private static readonly PackedScene Licenses;
	private static readonly PackedScene Host;
	private static readonly PackedScene ConnectMenu;
	private static readonly PackedScene WaitConnecting;
	private static readonly PackedScene PauseMenu;
	private static readonly PackedScene InventoryMenu;

	public static Menu Self;

	public Menu()
	{
		Self = this;
	}


	static Menu()
	{
		if(Engine.EditorHint) return;

		//All menu scene files are loaded on game startup
		Intro = GD.Load<PackedScene>("res://UI/Menu/Intro/Intro.tscn");
		Update = GD.Load<PackedScene>("res://UI/Menu/UpdateMenu/UpdateMenu.tscn");
		Nick = GD.Load<PackedScene>("res://UI/Menu/NickMenu/NickMenu.tscn");
		Main = GD.Load<PackedScene>("res://UI/Menu/MainMenu/MainMenu.tscn");
		Help = GD.Load<PackedScene>("res://UI/Menu/HelpMenu/HelpMenu.tscn");
		Credits = GD.Load<PackedScene>("res://UI/Menu/CreditsMenu/CreditsMenu.tscn");
		Licenses = GD.Load<PackedScene>("res://UI/Menu/LicensesMenu/LicensesMenu.tscn");
		Host = GD.Load<PackedScene>("res://UI/Menu/HostMenu/HostMenu.tscn");
		ConnectMenu = GD.Load<PackedScene>("res://UI/Menu/ConnectMenu/ConnectMenu.tscn");
		WaitConnecting = GD.Load<PackedScene>("res://UI/Menu/WaitConnectingMenu/WaitConnectingMenu.tscn");
		PauseMenu = GD.Load<PackedScene>("res://UI/Menu/PauseMenu/PauseMenu.tscn");
		InventoryMenu = GD.Load<PackedScene>("res://UI/Menu/InventoryMenu/InventoryMenu.tscn");
	}


	public static void Setup() //Called from Game.cs before this class's _Ready would
	{
		ShadedBackground = Game.RuntimeRoot.GetNode("MenuRoot").GetNode("ShadedBackground") as Panel;
		Center = Game.RuntimeRoot.GetNode("MenuRoot").GetNode("HBox/Center") as ScrollContainer;
	}


	public static void Reset()
	{
		ShadedBackground.Hide();

		if(Contents != null)
		{
			Contents.QueueFree();
			Contents = null;
		}

		IsOpen = true;
		IngameMenuOpen = false;
		Game.BindsEnabled = false;
		Input.SetMouseMode(Input.MouseMode.Visible);

		//Fake a release event of the left mouse button to cancel any drag operations
		var LeftMouseRelease = new InputEventMouseButton() {
			ButtonIndex = (int) ButtonList.Left,
			Pressed = false
		};
		Self.GetTree().InputEvent(LeftMouseRelease);
	}


	public static void Close()
	{
		Reset();
		IsOpen = false;
		IngameMenuOpen = false;

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


	public static void BuildUpdate()
	{
		Reset();

		Contents = Update.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildNick()
	{
		Reset();

		Contents = Nick.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildMain()
	{
		Reset();

		Contents = Main.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildHelp()
	{
		Reset();

		Contents = Help.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildCredits()
	{
		Reset();

		Contents = Credits.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildLicenses()
	{
		Reset();

		Contents = Licenses.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildHost()
	{
		Reset();

		Contents = Host.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildConnect()
	{
		Reset();

		Contents = ConnectMenu.Instance();
		Center.AddChild(Contents);
	}


	public static void BuildWaitConnecting()
	{
		Reset();

		Contents = WaitConnecting.Instance();
		Net.Self.Connect(nameof(Net.ConnectToFailed), Contents, "ConnectFailed");
		Center.AddChild(Contents);
	}


	public static void BuildPause()
	{
		Reset();

		Contents = PauseMenu.Instance();
		Center.AddChild(Contents);
		ShadedBackground.Show();
		IngameMenuOpen = true;
	}


	[SteelInputWithoutArg(typeof(Menu), nameof(BuildInventory))]
	public static void BuildInventory()
	{
		Reset();

		Contents = InventoryMenu.Instance();
		Center.AddChild(Contents);
		ShadedBackground.Show();
		IngameMenuOpen = true;
	}
}
