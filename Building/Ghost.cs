using Godot;
using System;
using System.Collections.Generic;

public class Ghost : Area
{
	Material GreenMat;
	Material RedMat;
	MeshInstance GhostMesh;

	private static Dictionary<Items.TYPE, Mesh> Meshes = new Dictionary<Items.TYPE, Mesh>();

	public Items.TYPE CurrentMeshType;
	public bool CanBuild = false;

	List<Items.TYPE> OldType;
	List<Vector3> OldPositions;
	List<Vector3> OldRotations;
	List<bool> OldVisible;
	List<bool> OldCanBuild;

	Ghost()
	{
		if(Engine.EditorHint) {return;}

		GreenMat = GD.Load("res://Building/Materials/GreenGhost.tres") as Material;
		RedMat = GD.Load("res://Building/Materials/RedGhost.tres") as Material;

		if(Meshes.Count <= 0)
			LoadMeshes();

		//Godot's `Area` object likes to not register body entry's for several
		  //physics ticks so these postion, rotation, and visibility queues
		  //are required to prevent flashes of the incorrect color/build abilty
		OldType = new List<Items.TYPE>()
			{
				Items.TYPE.ERROR,
				Items.TYPE.ERROR
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


	private static void LoadMeshes()
	{
		foreach(Items.TYPE Type in System.Enum.GetValues(typeof(Items.TYPE)))
		{
			File ToLoad = new File();
			if(ToLoad.FileExists("res://Items/Meshes/" + Type.ToString() + ".obj"))
			{
				Meshes.Add(Type, GD.Load("res://Items/Meshes/" + Type.ToString() + ".obj") as Mesh);
			}
			else
			{
				Meshes.Add(Type, GD.Load("res://Items/Meshes/ERROR.obj") as Mesh);
			}
		}
	}


	public override void _Ready()
	{
		GhostMesh = ((PackedScene)(GD.Load("res://Building/GhostMesh.tscn"))).Instance() as MeshInstance;
		GetParent().AddChild(GhostMesh);

		Items.Instance Item = Game.PossessedPlayer.Inventory[Game.PossessedPlayer.InventorySlot];
		if(Item != null) //null means no item in slot
		{
			GhostMesh.Mesh = Meshes[Item.Type];
			CurrentMeshType = Item.Type;
		}
	}


	public override void _PhysicsProcess(float Delta)
	{
		GhostMesh.Translation = OldPositions[0];
		GhostMesh.RotationDegrees = OldRotations[0];
		GhostMesh.Visible = OldVisible[0];

		GhostMesh.Mesh = Meshes[OldType[0]];
		CurrentMeshType = OldType[0];

		Player Plr = Game.PossessedPlayer;
		OldVisible.RemoveAt(0);
		OldVisible.Add(false);
		if(Plr.Inventory[Plr.InventorySlot] != null)
		{
			RayCast BuildRayCast = Plr.GetNode("SteelCamera/RayCast") as RayCast;
			if(BuildRayCast.IsColliding())
			{
				Structure Hit = BuildRayCast.GetCollider() as Structure;
				if(Hit != null)
				{
					System.Nullable<Vector3> GhostPosition = BuildPositions.Calculate(Hit, Plr.Inventory[Plr.InventorySlot].Type);
					if(GhostPosition != null)
					{
						Vector3 GhostRotation = BuildRotations.Calculate(Hit, Plr.Inventory[Plr.InventorySlot].Type);
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
				if(SelectedItem != null && Body is Structure && ((Structure)Body).Type == SelectedItem.Type)
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
		if(Item != null && Item.Type != CurrentMeshType) //null means no item in slot
		{
			OldType.RemoveAt(0);
			OldType.Add(Item.Type);
		}
	}
}
