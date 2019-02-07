using Godot;


public class Menu : Node
{
	private static MenuRoot Root;

	private static PackedScene MButton;

	static Menu()
	{
		if(Engine.EditorHint) {return;}

		MButton = GD.Load<PackedScene>("res://UI/Menu/MButton.tscn");
	}

	public static void Setup()
	{
		Root = Game.RuntimeRoot.GetNode("MenuRoot") as MenuRoot;
		Root.Center = Root.GetNode<VBoxContainer>("HBox/VCenter");
	}


	private static void Reset()
	{
		foreach(Node Child in Root.Center.GetChildren())
		{
			Child.QueueFree();
		}
	}


	public static void BuildMain()
	{
		Reset();
	}
}
