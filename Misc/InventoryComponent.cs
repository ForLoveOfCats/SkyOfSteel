using Godot;
using Optional;
using System;
using System.Collections.Generic;
using static Godot.Mathf;



public class SavedInventory
{
	[Newtonsoft.Json.JsonProperty("C")]
	public Items.Instance[] Contents;

	public SavedInventory()
	{}

	public SavedInventory(InventoryComponent Inventory)
	{
		Contents = Inventory.Contents;
	}
}



public class InventoryComponent
{
	public const int MaxStackCount = 50;


	public Items.Instance[] Contents;
	private IHasInventory Owner;


	public InventoryComponent(IHasInventory OwnerArg, int SlotCountArg)
	{
		Owner = OwnerArg;
		Contents = new Items.Instance[SlotCountArg];
	}


	public Items.Instance this[int Index]
	{
		get => Contents[Index];
		private set => Contents[Index] = value;
	}


	public Option<int[]> Give(Items.Instance ToGive) //TODO: Re-evaluate return values
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception("Attempted to give item on client");

		for(int Slot = 0; Slot < Contents.Length; Slot++)
		{
			if(Contents[Slot] is null || Contents[Slot].Id != ToGive.Id) continue;

			int GivingCount = Clamp(MaxStackCount - Contents[Slot].Count, 0, ToGive.Count);
			ToGive.Count -= GivingCount;
			Contents[Slot].Count += GivingCount;

			if(ToGive.Count <= 0)
			{
				Entities.SendInventory(Owner);
				return Option.Some(new int[] {Slot});
			}
		}

		var Slots = new List<int>();
		for(int Slot = 0; Slot < Contents.Length; Slot++)
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
				{
					Entities.SendInventory(Owner);
					return Option.Some(Slots.ToArray());
				}
			}
		}

		Entities.SendInventory(Owner);
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


	public void TransferTo(IHasInventory To, int FromSlot, int ToSlot, Items.IntentCount CountMode)
	{
		Assert.ActualAssert(Net.Work.IsNetworkServer());

		if(Contents[FromSlot] is Items.Instance Item)
		{
			int Count = Items.CalcRetrieveCount(CountMode, Item.Count);
			if(Count <= 0)
				return;

			if(To.Inventory[ToSlot] == null || To.Inventory[ToSlot].Id == Item.Id)
			{
				if(To.Inventory[ToSlot] == null)
					To.Inventory.UpdateSlot(ToSlot, Item.Id, Count);
				else
				{
					Count = Clamp(MaxStackCount - To.Inventory[ToSlot].Count, 0, Count);
					To.Inventory.UpdateSlot(ToSlot, Item.Id, To.Inventory[ToSlot].Count + Count);
				}

				if(Count <= 0)
					return;

				if(Item.Count == Count)
					Owner.Inventory.EmptySlot(FromSlot);
				else
					Owner.Inventory.UpdateSlot(FromSlot, Item.Id, Item.Count - Count);
			}
			else if(To.Inventory[ToSlot] != null && Item.Count == Count)
			{
				var OriginalAtTarget = To.Inventory[ToSlot];
				To.Inventory.UpdateSlot(ToSlot, Item.Id, Count);
				Owner.Inventory.UpdateSlot(FromSlot, OriginalAtTarget.Id, OriginalAtTarget.Count);
			}

			Entities.SendInventory(Owner);
			Entities.SendInventory(To);
		}
	}


	public void ThrowAt(int Slot, Items.IntentCount CountMode, Vector3 At, Vector3 Velocity)
	{
		if(Contents[Slot] is Items.Instance Item)
		{
			int Count = Items.CalcRetrieveCount(CountMode, Item.Count);
			if(Count <= 0)
				return;

			for(int Index = 0; Index < Count; Index += 1)
				World.Self.DropItem(Item.Id, At, Velocity);

			if(Count >= Item.Count) //Dropping entire stack
				EmptySlot(Slot);
			else
				UpdateSlot(Slot, Item.Id, Item.Count - Count);

			Entities.SendInventory(Owner);
		}
	}
}
