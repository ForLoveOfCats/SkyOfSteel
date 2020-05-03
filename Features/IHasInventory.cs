using Godot;



public interface IHasInventory {
	InventoryComponent Inventory { get; set; }

	string Name { get; set; }
	Vector3 Translation { get; set; }


	NodePath GetPath();
}
