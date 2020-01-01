using Godot;



public interface IHasInventory
{
	InventoryComponent Inventory { get; set; }


	[Remote]
	void NetUpdateInventorySlot(int Slot, Items.ID Id, int Count);


	[Remote]
	void NetEmptyInventorySlot(int Slot);
}
