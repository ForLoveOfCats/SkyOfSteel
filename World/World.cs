using Godot;
using System.Collections.Generic;


public class World : Node
{
	public static List<DroppedItem> DroppedItems = new List<DroppedItem>();

	private static PackedScene DroppedItemScene;

	public World()
	{
		if(Engine.EditorHint) {return;}

		DroppedItemScene = GD.Load<PackedScene>("res://Items/DroppedItem.tscn");
	}


	public static void DropItem(Items.Instance ItemInstance, Vector3 Position, Vector3 BaseMomentum)
	{
		DroppedItem ToDrop = DroppedItemScene.Instance() as DroppedItem;
		ToDrop.Translation = Position;
		ToDrop.Momentum = BaseMomentum;
		ToDrop.Item = ItemInstance;
		ToDrop.GetNode<MeshInstance>("MeshInstance").Mesh = Items.Meshes[ItemInstance.Type];

		DroppedItems.Add(ToDrop);
		Game.StructureRoot.AddChild(ToDrop);
	}
}
