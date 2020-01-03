using System.Diagnostics;
using Godot;



public class InventoryIcon : TextureRect
{
	public enum UsageCase {MENU, PREVIEW}

	public InventoryMenu ParentMenu = null;
	public IHasInventory Source = null;
	public int Slot = 0;
	public UsageCase Case;

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
		UpdateIcon();
	}


	public override object GetDragData(Vector2 Pos)
	{
		if(Source.Inventory[Slot] == null)
			return null;

		var Preview = (InventoryIcon) InventoryIconScene.Instance();
		Preview.Source = Source;
		Preview.Slot = Slot;
		SetDragPreview(Preview);

		InventoryMenu.DragMode Mode = InventoryMenu.DragMode.ALL;
		if(Input.IsKeyPressed((int) KeyList.Shift))
			Mode = InventoryMenu.DragMode.HALF;
		else if(Input.IsKeyPressed((int) Godot.KeyList.Control))
			Mode = InventoryMenu.DragMode.SINGLE;
		ParentMenu.Source = new InventoryMenu.SourceData(Source, Mode);

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

			//We know that the item being moved cannot be null
			Items.Instance Moving = ParentMenu.Source.Source.Inventory[FromSlot];
			Debug.Assert(Moving != null);
			Items.Instance Original = Source.Inventory[Slot];

			int RetrieveCount = 0;
			switch(ParentMenu.Source.Mode)
			{
				case InventoryMenu.DragMode.ALL:
					RetrieveCount = Moving.Count;
					break;
				case InventoryMenu.DragMode.HALF:
					if(Moving.Count == 1)
						RetrieveCount = 1;
					else
						RetrieveCount = Moving.Count / 2; //Relying on rounding down via truncation
					break;
				case InventoryMenu.DragMode.SINGLE:
					RetrieveCount = 1;
					break;
			}

			if(Original == null) //Replace
			{
				if(Net.Work.IsNetworkServer())
				{
					Source.NetUpdateInventorySlot(Slot, Moving.Id, RetrieveCount);
					ParentMenu.Source.Source.NetEmptyInventorySlot(FromSlot);
				}
				else
				{
					Source.RpcId(Net.ServerId, nameof(Source.NetUpdateInventorySlot), Slot, Moving.Id, RetrieveCount);
					ParentMenu.Source.Source.RpcId(Net.ServerId, nameof(ParentMenu.Source.Source.NetEmptyInventorySlot), FromSlot);
				}
			}
			else
			{
				if(Moving.Id == Original.Id) //Combine at target
				{
					if(Net.Work.IsNetworkServer())
					{
						Source.NetUpdateInventorySlot(Slot, Original.Id, Original.Count + RetrieveCount);
						ParentMenu.Source.Source.NetEmptyInventorySlot(FromSlot);
					}
					else
					{
						Source.RpcId(Net.ServerId, nameof(Source.NetUpdateInventorySlot), Slot, Original.Id, Original.Count + RetrieveCount);
						ParentMenu.Source.Source.RpcId(Net.ServerId, nameof(ParentMenu.Source.Source.NetEmptyInventorySlot), FromSlot);
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
						Source.RpcId(Net.ServerId, nameof(Source.NetUpdateInventorySlot), Slot, Moving.Id, Moving.Count);
						ParentMenu.Source.Source.RpcId(Net.ServerId, nameof(ParentMenu.Source.Source.NetUpdateInventorySlot), FromSlot, Original.Id, Original.Count);
					}
				}
			}
		}
	}

	public void UpdateIcon()
	{
		if(Source.Inventory[Slot] == null)
		{
			Texture = ParentMenu.Alpha;
			CountLabel.Text = "";
		}
		else
		{
			Texture = Items.Thumbnails[Source.Inventory[Slot].Id];
			CountLabel.Text = Source.Inventory[Slot].Count.ToString();
		}
	}


	public override void _Process(float Delta)
	{
		if(Case == UsageCase.MENU && GetParent() is BoxContainer Box)
		{
			float Height = Box.RectSize.y;
			RectMinSize = new Vector2(Height / 12, Height / 12);
		}
		else if(Case == UsageCase.PREVIEW)
		{
			float Height = GetViewport().GetVisibleRect().Size.y;
			RectMinSize = new Vector2(Height / 15, Height / 15);
		}
	}
}
