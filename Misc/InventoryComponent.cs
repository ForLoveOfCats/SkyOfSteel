using System;
using Godot;



public class InventoryComponent
{
	private Items.Instance[] Contents;

	private IHasInventory Owner;

	public readonly int SlotCount;


	public InventoryComponent(IHasInventory OwnerArg, int SlotCountArg, bool HiddenLast = false)
	{
		Contents = new Items.Instance[SlotCountArg];

		Owner = OwnerArg;

		if(HiddenLast)
			SlotCount = SlotCountArg - 1;
		else
			SlotCount = SlotCountArg;
	}


	public Items.Instance this[int Index]
	{
		get => Contents[Index];
		private set => Contents[Index] = value;
	}


	public int Give(Items.Instance ToGive)
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception("Attempted to give item on client");

		for(int Slot = 0; Slot < SlotCount; Slot++)
		{
			if(Contents[Slot] is null || Contents[Slot].Id != ToGive.Id) continue;

			Contents[Slot].Count += ToGive.Count;
			return Slot;
		}

		for(int Slot = 0; Slot < SlotCount; Slot++)
		{
			if(Contents[Slot] is null)
			{
				Contents[Slot] = ToGive;
				return Slot;
			}
		}

		return -1; //Full, TODO: Handle this better
	}


	public void UpdateSlot(int Slot, Items.ID Id, int Count)
	{
		Contents[Slot] = new Items.Instance(Id) {Count = Count};
	}


	public void EmptySlot(int Slot)
	{
		Contents[Slot] = null;
	}


	public void TransferTo(NodePath Path, int FromSlot, int ToSlot, Items.IntentCount CountMode)
	{
		if(Contents[FromSlot] is Items.Instance Item && Game.RuntimeRoot.GetNode(Path) is IHasInventory To)
		{
			int Count = Items.CalcRetrieveCount(CountMode, Item.Count);
			if(Count <= 0)
				return; //No item(s) were retrieved from the source, abort

			if(To.Inventory[ToSlot] == null || To.Inventory[ToSlot].Id == Item.Id)
			{
				if(To.Inventory[ToSlot] == null)
					To.NetUpdateInventorySlot(ToSlot, Item.Id, Count);
				else
					To.NetUpdateInventorySlot(ToSlot, Item.Id, To.Inventory[ToSlot].Count + Count);

				if(Item.Count == Count)
					Owner.NetEmptyInventorySlot(FromSlot);
				else
					Owner.NetUpdateInventorySlot(FromSlot, Item.Id, Item.Count - Count);
			}
			else if(To.Inventory[ToSlot] != null && Item.Count == Count)
			{
				var OriginalAtTarget = To.Inventory[ToSlot];
				To.NetUpdateInventorySlot(ToSlot, Item.Id, Count);
				Owner.NetUpdateInventorySlot(FromSlot, OriginalAtTarget.Id, OriginalAtTarget.Count);
			}
		}
	}
}
