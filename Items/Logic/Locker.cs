using Godot;
using static Godot.Mathf;
using static SteelMath;
using System.Collections.Generic;



public class Locker : PipeCoreLogic, IHasInventory
{
	private bool InitiallyFilledFriends = false;

	private Spatial Position1;

	private MeshInstance OpenEndMesh;
	private CollisionShape OpenEndCollision;
	private StaticBody OpenEnd;

	public InventoryComponent Inventory { get; set; } = new InventoryComponent(10);

	public override void _Ready()
	{
		System = new PipeSystem(this);
		Friends = new HashSet<PipeCoreLogic>();

		Position1 = GetNode<Spatial>("Positions/Position1");
		OpenEndMesh = GetNode<MeshInstance>("OpenEndMesh");
		OpenEndCollision = GetNode<CollisionShape>("OpenEndCollision");
		OpenEnd = GetNode<StaticBody>("OpenEnd");

		CallDeferred(nameof(GridUpdate));
	}


	public override void GridUpdate()
	{
		HashSet<PipeCoreLogic> OriginalFriends = Friends;
		Friends = new HashSet<PipeCoreLogic>();

		PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
		Godot.Collections.Dictionary Results;
		Results = State.IntersectRay(Translation, Position1.GlobalTransform.origin, new Godot.Collections.Array() { this, OpenEnd, Game.PossessedPlayer }, 2|4);
		if(Results.Count > 0 && Results["collider"] is OpenEnd)
		{
			OpenEndMesh.Show();
			OpenEndCollision.Disabled = false;
			System.Consume(((OpenEnd)Results["collider"]).Parent.System);
			Friends.Add(((OpenEnd)Results["collider"]).Parent);
		}
		else
		{
			OpenEndMesh.Hide();
			OpenEndCollision.Disabled = true;
		}

		if(InitiallyFilledFriends && !Friends.SetEquals(OriginalFriends))
		{
			System = new PipeSystem(this);
			RecursiveAddFriendsToSystem();
		}
		InitiallyFilledFriends = true;
	}


	public override void OnRemove()
	{
		for(int Index = 0; Index < Inventory.SlotCount; Index++)
		{
			if(Inventory[Index] is Items.Instance Item)
			{
				for(int C = 0; C < Item.Count; C++)
					World.Self.DropItem(Item.Id, Translation, new Vector3());
			}
		}

		base.OnRemove();
	}


	[Remote]
	public void NetUpdateInventorySlot(int Slot, Items.ID ItemId, int Count)
	{
		Inventory.UpdateSlot(Slot, ItemId, Count);

		if(Net.Work.IsNetworkServer())
			Net.SteelRpc(this, nameof(NetUpdateInventorySlot), Slot, ItemId, Count);
	}


	[Remote]
	public void NetEmptyInventorySlot(int Slot)
	{
		Inventory.EmptySlot(Slot);

		if(Net.Work.IsNetworkServer())
			Net.SteelRpc(this, nameof(NetEmptyInventorySlot), Slot);
	}
}
