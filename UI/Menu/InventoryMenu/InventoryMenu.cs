using Godot;



public class InventoryMenu : VBoxContainer
{
	public class FromData
	{
		public IHasInventory Source;
		public Items.IntentCount CountMode;

		public FromData(IHasInventory SourceArg, Items.IntentCount CountModeArg)
		{
			Source = SourceArg;
			CountMode = CountModeArg;
		}
	}


	public Texture Alpha;
	public PackedScene InventoryIconScene;

	public VBoxContainer PlayerVBox ;
	public InventoryIcon[] PlayerIcons;

	public IHasInventory Other; //Will be null if we are just the normal inventory screen
	public GridContainer OtherGrid;
	public InventoryIcon[] OtherIcons;

	public FromData From = null;


	public override void _Ready()
	{
		Alpha = GD.Load("res://UI/Textures/Alpha.png") as Texture;
		InventoryIconScene = GD.Load<PackedScene>("res://UI/InventoryIcon.tscn");

		PlayerVBox = GetNode<VBoxContainer>("HBoxContainer/PlayerCenter/PlayerVBox");
		OtherGrid = GetNode<GridContainer>("HBoxContainer/OtherVBox/OtherCenter/OtherGrid");

		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				PlayerIcons = new InventoryIcon[Plr.Inventory.SlotCount];
				for(int Index = 0; Index < Plr.Inventory.SlotCount - 1; Index++) //Ignore eleventh slot, used for dropping
				{
					InventoryIcon Icon = InstantiateIcon(Index, Plr);
					PlayerVBox.AddChild(Icon);
					PlayerIcons[Index] = Icon;
				}
			}
		);

		if(Other != null)
		{
			OtherIcons = new InventoryIcon[Other.Inventory.SlotCount];
			for(int Index = 0; Index < Other.Inventory.SlotCount; Index++)
			{
				InventoryIcon Icon = InstantiateIcon(Index, Other);
				OtherGrid.AddChild(Icon);
				OtherIcons[Index] = Icon;
			}
		}
	}


	public InventoryIcon InstantiateIcon(int SlotArg, IHasInventory SourceArg)
	{
		var Icon = (InventoryIcon) InventoryIconScene.Instance();

		Icon.ParentMenu = this;
		Icon.Slot = SlotArg;
		Icon.Source = SourceArg;
		Icon.Case = InventoryIcon.UsageCase.MENU;

		return Icon;
	}


	public override bool CanDropData(Vector2 Pos, object Data)
	{
		return Data is int;
	}


	public override void DropData(Vector2 Pos, object Data)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				if(Data is int FromSlot && From != null)
					From.Source.TransferTo(Plr.GetPath(), FromSlot, 10, From.CountMode);
			}
		);
	}
}
