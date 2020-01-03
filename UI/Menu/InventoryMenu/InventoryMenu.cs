using Godot;



public class InventoryMenu : VBoxContainer
{
	public class SourceData
	{
		public IHasInventory Source;
		public DragMode Mode;

		public SourceData(IHasInventory SourceArg, DragMode ModeArg)
		{
			Source = SourceArg;
			Mode = ModeArg;
		}
	}



	public enum DragMode {ALL, SINGLE, HALF};

	public Texture Alpha = null;
	public PackedScene InventoryIconScene = null;

	public VBoxContainer PlayerVBox = null;
	public InventoryIcon[] PlayerIcons = new InventoryIcon[10];

	public SourceData Source = null;

	public override void _Ready()
	{
		Alpha = GD.Load("res://UI/Textures/Alpha.png") as Texture;
		InventoryIconScene = GD.Load<PackedScene>("res://UI/InventoryIcon.tscn");

		PlayerVBox = GetNode<VBoxContainer>("HBoxContainer/PlayerVBox");

		for(int x = 0; x <= 9; x++)
		{
			var Icon = (InventoryIcon) InventoryIconScene.Instance();
			Icon.ParentMenu = this;
			Icon.Slot = x;
			Icon.Source = Game.PossessedPlayer;
			Icon.Case = InventoryIcon.UsageCase.MENU;

			PlayerVBox.AddChild(Icon);
			PlayerIcons[x] = Icon;
		}
	}
}
