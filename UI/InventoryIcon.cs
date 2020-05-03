using Godot;



public class InventoryIcon : TextureRect {
	public enum UsageCase { MENU, PREVIEW }

	public InventoryMenu ParentMenu = null;
	public IHasInventory Source = null;
	public int Slot = 0;
	public UsageCase Case;

	public Items.ID CurrentId;

	public Label CountLabel;

	private static PackedScene InventoryIconScene;

	static InventoryIcon() {
		if(Engine.EditorHint) { return; }

		InventoryIconScene = GD.Load<PackedScene>("res://UI/InventoryIcon.tscn");
	}


	public override void _Ready() {
		CountLabel = GetNode<Label>("Label");
		CountLabel.Text = "";

		UpdateIcon();
	}


	public override void _Draw() {
		if(Case == UsageCase.MENU) {
			var Rect = new Rect2(new Vector2(), RectSize);
			DrawRect(Rect, new Color(1, 1, 1), false, 1);
		}
	}


	public override object GetDragData(Vector2 Pos) {
		if(Source.Inventory[Slot] == null)
			return null;

		Items.IntentCount Mode = Items.IntentCount.ALL;
		if(Input.IsKeyPressed((int)KeyList.Shift))
			Mode = Items.IntentCount.HALF;
		else if(Input.IsKeyPressed((int)Godot.KeyList.Control))
			Mode = Items.IntentCount.SINGLE;
		ParentMenu.From = new InventoryMenu.FromData(Source, Mode);

		var Preview = (InventoryIcon)InventoryIconScene.Instance();
		Preview.ParentMenu = ParentMenu;
		Preview.Source = Source;
		Preview.Slot = Slot;
		Preview.Case = UsageCase.PREVIEW;
		SetDragPreview(Preview);

		return Slot;
	}


	public override bool CanDropData(Vector2 Pos, object Data) {
		return Data is int;
	}


	public override void DropData(Vector2 Pos, object Data) {
		if(Data is int FromSlot && ParentMenu.From != null) {
			if(Source == ParentMenu.From.Source && Slot == FromSlot)
				return; //Same source and slot, we dropped on ourself

			Entities.TransferFromTo(ParentMenu.From.Source, FromSlot, Source, Slot, ParentMenu.From.CountMode);
		}
	}

	public void UpdateIcon() {
		if(Source.Inventory[Slot] == null) {
			CurrentId = Items.ID.NONE;
			Texture = ParentMenu.Alpha;
		}
		else {
			CurrentId = Source.Inventory[Slot].Id;
			Texture = Items.Thumbnails[CurrentId];
		}
	}


	public override void _Process(float Delta) {
		if(Source.Inventory[Slot] is Items.Instance NotNull) {
			if(NotNull.Id != CurrentId)
				UpdateIcon();

			int Count = 0;
			if(Case == UsageCase.MENU)
				Count = NotNull.Count;
			else if(Case == UsageCase.PREVIEW)
				Count = Items.CalcRetrieveCount(ParentMenu.From.CountMode, NotNull.Count);

			CountLabel.Text = Count.ToString();
		}
		else if(CurrentId != Items.ID.NONE) {
			UpdateIcon();
			CountLabel.Text = "";
		}

		if(Case == UsageCase.MENU && GetParent().GetParent() is CenterContainer Cont) {
			float Height = Cont.RectSize.y;
			RectMinSize = new Vector2(Height / 11f, Height / 11f);
		}
		else if(Case == UsageCase.PREVIEW) {
			float Height = GetViewport().GetVisibleRect().Size.y;
			RectSize = new Vector2(Height / 9f, Height / 9f);
		}
	}
}
