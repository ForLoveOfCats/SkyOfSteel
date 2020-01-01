using Godot;



public class InventoryIcon : TextureButton
{
	public InventoryMenu ParentMenu = null;
	public IHasInventory Source = null;
	public int Slot = 0;


	public override void _Pressed()
	{
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
