using Godot;



public class InventoryMenu : VBoxContainer
{
	public Texture Alpha = null;
	public PackedScene InventoryIconScene = null;

	public VBoxContainer PlayerVBox = null;
	public InventoryIcon[] PlayerIcons = new InventoryIcon[10];

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

			PlayerVBox.AddChild(Icon);
			PlayerIcons[x] = Icon;

			Icon.UpdateIcon();
			Icon.CallDeferred(nameof(Icon.UpdateSize)); //The container's size will not populate to non-default values until next frame
		}
	}
}
