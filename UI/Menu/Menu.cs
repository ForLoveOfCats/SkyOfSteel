using Godot;


public class Menu : Node
{
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
		//ScrollContainer spawns two scrollbar children so any contents would be the third child
		if(Center.GetChildCount() > 2)
		{
			Center.GetChild(2).Free(); //Could be dangerous to Free instead of QueueFree
		}
	}


	public static void BuildIntro()
	{
		Reset();
		Center.AddChild(Intro.Instance());
	}
}
