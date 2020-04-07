using Godot;
using System.Linq;



public class Ghost : Area
{
	Material GreenMat;
	Material RedMat;
	MeshInstance GhostMesh;

	public Items.ID CurrentMeshType = Items.ID.NONE;
	public bool CanBuild = false;

	Ghost()
	{
		if(Engine.EditorHint) {return;}

		GreenMat = GD.Load("res://World/Materials/GreenGhost.tres") as Material;
		RedMat = GD.Load("res://World/Materials/RedGhost.tres") as Material;
	}


	public override void _Ready()
	{
		GhostMesh = (MeshInstance) GD.Load<PackedScene>("res://World/GhostMesh.tscn").Instance();
		GetParent().AddChild(GhostMesh);
	}


	public override void _Process(float Delta)
	{
		Game.PossessedPlayer.Match(
			none: () => GhostMesh.Visible = false,

			some: (Plr) =>
			{
				if(Plr.Inventory[Plr.InventorySlot] == null)
				{
					GhostMesh.Visible = false;
					CurrentMeshType = Items.ID.NONE;
					return;
				}

				CurrentMeshType = Plr.Inventory[Plr.InventorySlot].Id;
				GhostMesh.Mesh = Items.Meshes[CurrentMeshType];

				var BuildRayCast = Plr.GetNode<RayCast>("SteelCamera/RayCast");
				if(BuildRayCast.IsColliding() && BuildRayCast.GetCollider() is Tile Base)
				{
					Vector3? GhostPosition = Items.TryCalculateBuildPosition(CurrentMeshType, Base, Plr.RotationDegrees.y, Plr.BuildRotation, BuildRayCast.GetCollisionPoint());
					if(GhostPosition != null)
					{
						GhostMesh.Visible = true;

						Vector3 GhostRotation = Items.CalculateBuildRotation(CurrentMeshType, Base, Plr.RotationDegrees.y, Plr.BuildRotation, BuildRayCast.GetCollisionPoint());

						Translation = (Vector3) GhostPosition;
						RotationDegrees = GhostRotation;
						GhostMesh.Translation = (Vector3) GhostPosition;
						GhostMesh.RotationDegrees = GhostRotation;

						CanBuild = true;
						if(GetOverlappingBodies().Count > 0)
						{
							foreach(Node Body in GetOverlappingBodies())
							{
								Items.ID[] DisallowedCollisions = Items.IdInfos[CurrentMeshType].DisallowedCollisions;
								if(DisallowedCollisions != null && Body is Tile Branch && DisallowedCollisions.Contains(Branch.ItemId))
									CanBuild = false;
							}
						}

						if(CanBuild)
							GhostMesh.MaterialOverride = GreenMat;
						else
							GhostMesh.MaterialOverride = RedMat;

						return;
					}
				}

				CanBuild = false;
				GhostMesh.Visible = false;
			}
		);
	}
}
