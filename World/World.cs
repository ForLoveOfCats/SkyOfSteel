using Godot;
using System.Collections.Generic;


public class World : Node
{
	public static List<DroppedItem> DroppedItems = new List<DroppedItem>();

	private static PackedScene DroppedItemScene;

	public static World Self;

	public World()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		DroppedItemScene = GD.Load<PackedScene>("res://Items/DroppedItem.tscn");
	}


	//Should be able to be called without rpc yet only run on server
	[Remote]
	public void DropItem(Items.TYPE Type, Vector3 Position, Vector3 BaseMomentum)
	{
		if(Self.GetTree().GetNetworkPeer() != null)
		{
			if(Self.GetTree().IsNetworkServer())
			{
				Net.SteelRpc(Self, nameof(NetDropItem), Type, Position, BaseMomentum);
				NetDropItem(Type, Position, BaseMomentum);
			}
			else
			{
				Self.RpcId(Net.ServerId, nameof(DropItem), Type, Position, BaseMomentum);
			}
		}
	}


	[Remote]
	public void NetDropItem(Items.TYPE Type, Vector3 Position, Vector3 BaseMomentum)
	{
		DroppedItem ToDrop = DroppedItemScene.Instance() as DroppedItem;
		ToDrop.Translation = Position;
		ToDrop.Momentum = BaseMomentum;
		ToDrop.Type = Type;
		ToDrop.GetNode<MeshInstance>("MeshInstance").Mesh = Items.Meshes[Type];

		DroppedItems.Add(ToDrop);
		Game.StructureRoot.AddChild(ToDrop);
	}
}
