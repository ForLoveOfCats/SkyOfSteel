using Godot;
using System;
using System.Collections.Generic;

public class Ghost : Area
{
	Material GreenMat;
	Material RedMat;
	MeshInstance GhostMesh;

	public Items.ID CurrentMeshType;
	public bool CanBuild = false;

	List<Items.ID> OldType;
	List<Vector3> OldPositions;
	List<Vector3> OldRotations;
	List<bool> OldVisible;
	List<bool> OldCanBuild;

	Ghost()
	{
		if(Engine.EditorHint) {return;}

		GreenMat = GD.Load("res://World/Materials/GreenGhost.tres") as Material;
		RedMat = GD.Load("res://World/Materials/RedGhost.tres") as Material;

		//Godot's `Area` object likes to not register body entry's for several
		  //physics ticks so these postion, rotation, and visibility queues
		  //are required to prevent flashes of the incorrect color/build abilty
		OldType = new List<Items.ID>()
			{
				Items.ID.ERROR,
				Items.ID.ERROR
			};
		OldPositions = new List<Vector3>()
			{
				new Vector3(0,0,0),
				new Vector3(0,0,0)
			};
		OldRotations = new List<Vector3>()
			{
				new Vector3(0,0,0),
				new Vector3(0,0,0)
			};
		OldVisible = new List<bool>()
			{
				false,
				false,
			};
		OldCanBuild = new List<bool>()
			{
				false,
				false,
			};
	}


	public override void _Ready()
	{
		GhostMesh = ((PackedScene)(GD.Load("res://World/GhostMesh.tscn"))).Instance() as MeshInstance;
		GetParent().AddChild(GhostMesh);

		Items.Instance Item = Game.PossessedPlayer.Inventory[Game.PossessedPlayer.InventorySlot];
 		if(Item != null) //null means no item in slot
		{
			GhostMesh.Mesh = Items.Meshes[Item.Id];
			CurrentMeshType = Item.Id;
		}
	}


	public override void _PhysicsProcess(float Delta)
	{
		GhostMesh.Translation = OldPositions[0];
		GhostMesh.RotationDegrees = OldRotations[0];
		GhostMesh.Visible = OldVisible[0];

		GhostMesh.Mesh = Items.Meshes[OldType[0]];
		CurrentMeshType = OldType[0];

		Player Plr = Game.PossessedPlayer;
		OldVisible.RemoveAt(0);
		OldVisible.Add(false);
		if(Plr.Inventory[Plr.InventorySlot] != null)
		{
			RayCast BuildRayCast = Plr.GetNode("SteelCamera/RayCast") as RayCast;
			if(BuildRayCast.IsColliding())
			{
				Tile Base = BuildRayCast.GetCollider() as Tile;
				if(Base != null)
				{
					Vector3? GhostPosition = Items.TryCalculateBuildPosition(CurrentMeshType, Base, Plr.RotationDegrees.y, Plr.BuildRotation, BuildRayCast.GetCollisionPoint());
					if(GhostPosition != null)
					{
						Vector3 GhostRotation = Items.CalculateBuildRotation(CurrentMeshType, Base, Plr.RotationDegrees.y, Plr.BuildRotation, BuildRayCast.GetCollisionPoint());
						Translation = (Vector3)GhostPosition;
						RotationDegrees = GhostRotation;
						OldVisible[1] = true;
					}
				}
			}
		}
		if(OldVisible[1] == false)
		{
			OldVisible[0] = false;
			GhostMesh.Visible = false;
		}

		OldCanBuild.RemoveAt(0);
		if(GetOverlappingBodies().Count > 0)
		{
			bool _CanBuild = true;
			foreach(Node Body in GetOverlappingBodies())
			{
				Items.Instance SelectedItem = Game.PossessedPlayer.Inventory[Game.PossessedPlayer.InventorySlot];
				if(SelectedItem != null && Body is Tile && ((Tile)Body).Type == SelectedItem.Id)
				{
					GhostMesh.MaterialOverride = RedMat;
					_CanBuild = false;
				}
			}
			OldCanBuild.Add(_CanBuild);
			if(_CanBuild)
			{
				GhostMesh.MaterialOverride = GreenMat;
			}
		}
		else
		{
			GhostMesh.MaterialOverride = GreenMat;
			OldCanBuild.Add(true);
		}
		CanBuild = OldCanBuild[0];

		OldPositions.RemoveAt(0);
		OldPositions.Add(Translation);
		OldRotations.RemoveAt(0);
		OldRotations.Add(RotationDegrees);

		Items.Instance Item = Game.PossessedPlayer.Inventory[Game.PossessedPlayer.InventorySlot];
		if(Item != null && Item.Id != CurrentMeshType) //null means no item in slot
		{
			OldType.RemoveAt(0);
			OldType.Add(Item.Id);
		}
	}
}
