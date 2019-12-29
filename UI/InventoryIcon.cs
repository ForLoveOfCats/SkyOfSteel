using Godot;



public class InventoryIcon : TextureButton
{
	public InventoryMenu ParentMenu = null;
	public IInventory Source = null;
	public int Slot = 0;


	public override void _Pressed()
	{
		if(ParentMenu.SourceButton == null && Source.Inventory[Slot] != null)
			ParentMenu.SourceButton = this;

		else if(ParentMenu.SourceButton != null && ParentMenu.SourceButton != this  && Source.Inventory[Slot] == null)
		{
			Source.Inventory[Slot] = ParentMenu.SourceButton.Source.Inventory[ParentMenu.SourceButton.Slot];
			ParentMenu.SourceButton.Source.Inventory[ParentMenu.SourceButton.Slot] = null;

			ParentMenu.SourceButton.UpdateIcon();
			ParentMenu.SourceButton = null;

			UpdateIcon();
			Game.PossessedPlayer.HUDInstance.HotbarUpdate();
		}

		else if(ParentMenu.SourceButton != null && ParentMenu.SourceButton != this && Source.Inventory[Slot] != null)
		{
			Items.Instance Original = Source.Inventory[Slot];
			Source.Inventory[Slot] = ParentMenu.SourceButton.Source.Inventory[ParentMenu.SourceButton.Slot];
			ParentMenu.SourceButton.Source.Inventory[ParentMenu.SourceButton.Slot] = Original;

			ParentMenu.SourceButton.UpdateIcon();
			ParentMenu.SourceButton = null;

			UpdateIcon();
			Game.PossessedPlayer.HUDInstance.HotbarUpdate();
		}
	}


	public void UpdateIcon()
	{
		if(Source.Inventory[Slot] == null)
			TextureNormal = ParentMenu.Alpha;

		else
			TextureNormal = Items.Thumbnails[Source.Inventory[Slot].Id];

		UpdateSize();
	}


	public void UpdateSize()
	{
		float VBoxHeight = ((VBoxContainer)GetParent()).RectSize.y;
		RectMinSize = new Vector2(VBoxHeight/11, VBoxHeight/11);
	}
}
