using System;
using Godot;



public class InventoryComponent
{
	private Items.Instance[] Contents;

	public readonly int SlotCount;


	public InventoryComponent(int SlotCountArg)
	{
		SlotCount = SlotCountArg;
		Contents = new Items.Instance[SlotCount];
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
}
