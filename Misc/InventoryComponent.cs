using Godot;
using Optional;
using System;
using System.Collections.Generic;
using static Godot.Mathf;



public class InventoryComponent
{
	public const int MaxStackCount = 50;


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


	public Option<int[]> Give(Items.Instance ToGive)
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception("Attempted to give item on client");

		for(int Slot = 0; Slot < SlotCount; Slot++)
		{
			if(Contents[Slot] is null || Contents[Slot].Id != ToGive.Id) continue;

			int GivingCount = Clamp(MaxStackCount - Contents[Slot].Count, 0, ToGive.Count);
			ToGive.Count -= GivingCount;
			Contents[Slot].Count += GivingCount;

			if(ToGive.Count <= 0)
				return Option.Some(new int[] {Slot});
		}

		var Slots = new List<int>();
		for(int Slot = 0; Slot < SlotCount; Slot++)
		{
			if(Contents[Slot] is null)
			{
				int GivingCount = Clamp(ToGive.Count, 0, MaxStackCount);
				ToGive.Count -= GivingCount;
				Contents[Slot] = new Items.Instance(ToGive.Id) {
					Count = GivingCount,
				};

				Slots.Add(Slot);

				if(ToGive.Count <= 0)
					return Option.Some(Slots.ToArray());
			}
		}

		return Option.None<int[]>(); //Full inventory and items left to give
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
				return;

			if(To.Inventory[ToSlot] == null || To.Inventory[ToSlot].Id == Item.Id)
			{
				if(To.Inventory[ToSlot] == null)
					To.NetUpdateInventorySlot(ToSlot, Item.Id, Count);
				else
				{
					Count = Clamp(MaxStackCount - To.Inventory[ToSlot].Count, 0, Count);
					To.NetUpdateInventorySlot(ToSlot, Item.Id, To.Inventory[ToSlot].Count + Count);
				}

				if(Count <= 0)
					return;

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
