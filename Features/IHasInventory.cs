using Godot;



public interface IHasInventory
{
	InventoryComponent Inventory { get; set; }


	NodePath GetPath();
	object RpcId(int peerId, string method, params object[] args);


	[Remote]
	void NetUpdateInventorySlot(int Slot, Items.ID Id, int Count);


	[Remote]
	void NetEmptyInventorySlot(int Slot);


	[Remote]
	void TransferTo(NodePath Path, int FromSlot, int ToSlot, Items.IntentCount CountMode);
}
