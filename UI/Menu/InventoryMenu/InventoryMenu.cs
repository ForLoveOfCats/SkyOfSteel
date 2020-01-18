using Godot;



public class InventoryMenu : VBoxContainer
{
	public class SourceData
	{
		public IHasInventory Source;
		public Items.IntentCount CountMode;

		public SourceData(IHasInventory SourceArg, Items.IntentCount CountModeArg)
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

	public SourceData Source = null;


	public override void _Ready()
	{
		Alpha = GD.Load("res://UI/Textures/Alpha.png") as Texture;
		InventoryIconScene = GD.Load<PackedScene>("res://UI/InventoryIcon.tscn");

		PlayerVBox = GetNode<VBoxContainer>("HBoxContainer/PlayerCenter/PlayerVBox");
		OtherGrid = GetNode<GridContainer>("HBoxContainer/OtherVBox/OtherCenter/OtherGrid");

		Player Plr = Game.PossessedPlayer;
		PlayerIcons = new InventoryIcon[Plr.Inventory.SlotCount];
		for(int Index = 0; Index < Plr.Inventory.SlotCount; Index++)
		{
			InventoryIcon Icon = InstantiateIcon(Index, Plr);
			PlayerVBox.AddChild(Icon);
			PlayerIcons[Index] = Icon;
		}

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


	//TODO: Remove, was moved to Items
	//Is still used here for dropping stacks
	public int CalcRetrieveCount(int Value)
	{
		switch(Source.CountMode)
		{
			case Items.IntentCount.ALL:
				//Keep original count as original
				break;
			case Items.IntentCount.HALF:
				if(Value != 1)
					Value /= 2; //Relying on rounding down via truncation
				break;
			case Items.IntentCount.SINGLE:
				Value = 1;
				break;
		}

		return Value;
	}


	public override bool CanDropData(Vector2 Pos, object Data)
	{
		return Data is int;
	}


	public override void DropData(Vector2 Pos, object Data)
	{
		if(Data is int FromSlot && Source != null)
		{
			Items.Instance Moving = Source.Source.Inventory[FromSlot];
			int RetrieveCount = CalcRetrieveCount(Moving.Count);
			Vector3 StartPos = Game.PossessedPlayer.Translation + Game.PossessedPlayer.Cam.Translation;

			for(int Index = 0; Index < RetrieveCount; Index++)
				World.Self.DropItem(Moving.Id, StartPos, Game.PossessedPlayer.CalcThrowVelocity());

			bool EmptyMoving = RetrieveCount == Moving.Count; //If we have thrown all, empty the the source slot
			if(Net.Work.IsNetworkServer())
			{
				if(EmptyMoving)
					Source.Source.NetEmptyInventorySlot(FromSlot);
				else
					Source.Source.NetUpdateInventorySlot(FromSlot, Moving.Id, Moving.Count - RetrieveCount);
			}
			else
			{
				if(EmptyMoving)
					Source.Source.RpcId(Net.ServerId, nameof(IHasInventory.NetEmptyInventorySlot), FromSlot);
				else
					Source.Source.RpcId(Net.ServerId, nameof(IHasInventory.NetUpdateInventorySlot), FromSlot, Moving.Id, Moving.Count - RetrieveCount);
			}
		}
	}
}
