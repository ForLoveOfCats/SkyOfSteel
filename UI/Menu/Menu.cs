using Godot;


public class Menu : Node
{
	public static bool IsOpen = true;


	private static ScrollContainer Center;

	private static PackedScene Intro;

	static Menu()
	{
		if(Engine.EditorHint) {return;}

		//All menu scene files are loaded on game startup
		Intro = GD.Load<PackedScene>("res://UI/Menu/Intro/Intro.tscn");
	}

	public static void Setup() //Called from Game.cs before this class's _Ready would
	{
		Center = Game.RuntimeRoot.GetNode("MenuRoot").GetNode("HBox/Center") as ScrollContainer;
	}


	private static void Reset()
	{
		if(Center.GetChildCount() > 2) //ScrollContainer spawns two scrollbar children
		{
			//However the actual contents are the first item for some reason
			Center.GetChild(0).Free(); //Could be dangerous to Free instead of QueueFree
		}
	}


	public static void Close()
	{
		Reset();
		IsOpen = false;

		if(!Console.IsOpen)
		{
			Game.BindsEnabled = true;
			Input.SetMouseMode(Input.MouseMode.Captured);
		}
	}


	private static void ResetBools()
	{
		IsOpen = true;
		Game.BindsEnabled = false;
		Input.SetMouseMode(Input.MouseMode.Visible);
	}


	public static void BuildIntro()
	{
		Reset();
		ResetBools();

		Center.AddChild(Intro.Instance());
	}
}
