using Godot;



public class InventoryIcon : TextureRect
{
	public enum UsageCase {MENU, PREVIEW}

	public InventoryMenu ParentMenu = null;
	public IHasInventory Source = null;
	public int Slot = 0;
	public UsageCase Case;

	public Items.ID CurrentId;

	public Label CountLabel;

	private static PackedScene InventoryIconScene;

	static InventoryIcon()
	{
		if(Engine.EditorHint) {return;}

		InventoryIconScene = GD.Load<PackedScene>("res://UI/InventoryIcon.tscn");
	}


	public override void _Ready()
	{
		CountLabel = GetNode<Label>("Label");
		CountLabel.Text = "";

		UpdateIcon();
	}


	public override void _Draw()
	{
		if(Case == UsageCase.MENU)
		{
			var Rect = new Rect2(new Vector2(), RectSize);
			DrawRect(Rect, new Color(1, 1, 1), false, 1);
		}
	}


	public override object GetDragData(Vector2 Pos)
	{
		if(Source.Inventory[Slot] == null)
			return null;

		InventoryMenu.DragMode Mode = InventoryMenu.DragMode.ALL;
		if(Input.IsKeyPressed((int) KeyList.Shift))
			Mode = InventoryMenu.DragMode.HALF;
		else if(Input.IsKeyPressed((int) Godot.KeyList.Control))
			Mode = InventoryMenu.DragMode.SINGLE;
		ParentMenu.Source = new InventoryMenu.SourceData(Source, Mode);

		var Preview = (InventoryIcon) InventoryIconScene.Instance();
		Preview.ParentMenu = ParentMenu;
		Preview.Source = Source;
		Preview.Slot = Slot;
		Preview.Case = UsageCase.PREVIEW;
		SetDragPreview(Preview);

		return Slot;
	}


	public override bool CanDropData(Vector2 Pos, object Data)
	{
		return Data is int;
	}


	public override void DropData(Vector2 Pos, object Data)
	{
		if(Data is int FromSlot && ParentMenu.Source != null)
		{
			if(Source == ParentMenu.Source.Source && Slot == FromSlot)
				return; //Same source and slot, we dropped on ourself

			Items.Instance Moving = ParentMenu.Source.Source.Inventory[FromSlot];
			Assert.ActualAssert(Moving != null);
			if(Moving == null) return; //Makes Roslyn happy

			Items.Instance Original = Source.Inventory[Slot];

			int RetrieveCount = ParentMenu.CalcRetrieveCount(Moving.Count);
			bool EmptyMoving = RetrieveCount == Moving.Count; //If we are moving all, empty the the source slot

			if(Original == null) //Replace (no item at target)
			{
				if(Net.Work.IsNetworkServer())
				{
					Source.NetUpdateInventorySlot(Slot, Moving.Id, RetrieveCount);
					if(EmptyMoving)
						ParentMenu.Source.Source.NetEmptyInventorySlot(FromSlot);
					else
						ParentMenu.Source.Source.NetUpdateInventorySlot(FromSlot, Moving.Id, Moving.Count - RetrieveCount);
				}
				else
				{
					Source.RpcId(Net.ServerId, nameof(IHasInventory.NetUpdateInventorySlot), Slot, Moving.Id, RetrieveCount);
					if(EmptyMoving)
						ParentMenu.Source.Source.RpcId(Net.ServerId, nameof(IHasInventory.NetEmptyInventorySlot), FromSlot);
					else
						ParentMenu.Source.Source.RpcId(Net.ServerId, nameof(IHasInventory.NetUpdateInventorySlot), FromSlot, Moving.Id, Moving.Count - RetrieveCount);
				}
			}
			else
			{
				if(Moving.Id == Original.Id) //Combine at target
				{
					if(Net.Work.IsNetworkServer())
					{
						Source.NetUpdateInventorySlot(Slot, Original.Id, Original.Count + RetrieveCount);
						if(EmptyMoving)
							ParentMenu.Source.Source.NetEmptyInventorySlot(FromSlot);
						else
							ParentMenu.Source.Source.NetUpdateInventorySlot(FromSlot, Moving.Id, Moving.Count - RetrieveCount);
					}
					else
					{
						Source.RpcId(Net.ServerId, nameof(IHasInventory.NetUpdateInventorySlot), Slot, Original.Id, Original.Count + RetrieveCount);
						if(EmptyMoving)
							ParentMenu.Source.Source.RpcId(Net.ServerId, nameof(IHasInventory.NetEmptyInventorySlot), FromSlot);
						else
							ParentMenu.Source.Source.RpcId(Net.ServerId, nameof(IHasInventory.NetUpdateInventorySlot), FromSlot, Moving.Id, Moving.Count - RetrieveCount);
					}
				}
				else if(RetrieveCount == Moving.Count) //Swap, only if we are moving all from the source slot
				{
					if(Net.Work.IsNetworkServer())
					{
						Source.NetUpdateInventorySlot(Slot, Moving.Id, Moving.Count);
						ParentMenu.Source.Source.NetUpdateInventorySlot(FromSlot, Original.Id, Original.Count);
					}
					else
					{
						Source.RpcId(Net.ServerId, nameof(IHasInventory.NetUpdateInventorySlot), Slot, Moving.Id, Moving.Count);
						ParentMenu.Source.Source.RpcId(Net.ServerId, nameof(IHasInventory.NetUpdateInventorySlot), FromSlot, Original.Id, Original.Count);
					}
				}
			}
		}
	}

	public void UpdateIcon()
	{
		if(Source.Inventory[Slot] == null)
		{
			CurrentId = Items.ID.NONE;
			Texture = ParentMenu.Alpha;
		}
		else
		{
			CurrentId = Source.Inventory[Slot].Id;
			Texture = Items.Thumbnails[CurrentId];
		}
	}


	public override void _Process(float Delta)
	{
		if(Source.Inventory[Slot] is Items.Instance NotNull)
		{
			if(NotNull.Id != CurrentId)
				UpdateIcon();

			int Count = 0;
			if(Case == UsageCase.MENU)
				Count = NotNull.Count;
			else if(Case == UsageCase.PREVIEW)
				Count = ParentMenu.CalcRetrieveCount(NotNull.Count);

			CountLabel.Text = Count.ToString();
		}
		else if(CurrentId != Items.ID.NONE)
		{
			UpdateIcon();
			CountLabel.Text = "";
		}

		if(Case == UsageCase.MENU && GetParent().GetParent() is CenterContainer Cont)
		{
			float Height = Cont.RectSize.y;
			RectMinSize = new Vector2(Height / 12, Height / 12);
		}
		else if(Case == UsageCase.PREVIEW)
		{
			float Height = GetViewport().GetVisibleRect().Size.y;
			RectSize = new Vector2(Height / 18f, Height / 18f);
		}
	}
}
