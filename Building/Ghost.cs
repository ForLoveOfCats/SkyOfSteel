using Godot;
using System;
using System.Collections.Generic;

public class Ghost : Area
{
	Material GreenMat;
	Material RedMat;
	MeshInstance GhostMesh;

	private static Dictionary<Items.TYPE, Mesh> Meshes = new Dictionary<Items.TYPE, Mesh>();
	Items.TYPE CurrentMeshType;


	public bool CanBuild = false;

	System.Collections.Generic.List<Vector3> OldPositions;
	System.Collections.Generic.List<Vector3> OldRotations;
	System.Collections.Generic.List<bool> OldVisible;
	System.Collections.Generic.List<bool> OldCanBuild;

	Ghost()
	{
		GreenMat = GD.Load("res://Building/Materials/GreenGhost.tres") as Material;
		RedMat = GD.Load("res://Building/Materials/RedGhost.tres") as Material;

		if(Meshes.Count <= 0)
			LoadMeshes();

		//Godot's `Area` object likes to not register body entry's for several
		  //physics ticks so these postion, rotation, and visibility queues
		  //are required to prevent flashes of the incorrect color/build abilty
		OldPositions = new System.Collections.Generic.List<Vector3>()
			{
				new Vector3(0,0,0),
				new Vector3(0,0,0)
			};
		OldRotations = new System.Collections.Generic.List<Vector3>()
			{
				new Vector3(0,0,0),
				new Vector3(0,0,0)
			};
		OldVisible = new System.Collections.Generic.List<bool>()
			{
				false,
				false,
			};
		OldCanBuild = new System.Collections.Generic.List<bool>()
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
			if(ToLoad.FileExists("res://Building/Meshes/" + Type.ToString() + ".obj"))
			{
				Meshes.Add(Type, GD.Load("res://Building/Meshes/" + Type.ToString() + ".obj") as Mesh);
			}
			else
			{
				Meshes.Add(Type, GD.Load("res://Building/Meshes/ERROR.obj") as Mesh);
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
		Items.Instance Item = Game.PossessedPlayer.Inventory[Game.PossessedPlayer.InventorySlot];
		if(Item != null && Item.Type != CurrentMeshType) //null means no item in slot
		{
			GhostMesh.Mesh = Meshes[Item.Type];
			CurrentMeshType = Item.Type;
		}

		GhostMesh.Translation = OldPositions[0];
		GhostMesh.RotationDegrees = OldRotations[0];
		GhostMesh.Visible = OldVisible[0];

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
			GhostMesh.MaterialOverride = RedMat;
			OldCanBuild.Add(false);
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
	}
}
