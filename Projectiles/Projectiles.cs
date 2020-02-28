using Godot;
using System.Collections.Generic;
using static Godot.Mathf;



public class Projectiles : Node
{
	public struct ProjectileData
	{
		public PackedScene Scene;
		public float InitialSpeed;
	}


	public enum ProjectileID
	{
		ROCKET_JUMPER
	}

	public static Dictionary<ProjectileID, ProjectileData> Data;


	public static Projectiles Self;

	private Projectiles()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		Data = new Dictionary<ProjectileID, ProjectileData> {
			{
				ProjectileID.ROCKET_JUMPER,
				new ProjectileData {
					Scene = GD.Load<PackedScene>("Items/Logic/RocketJumper/JumperRocket.tscn"),
					InitialSpeed = 150
				}
			}
		};
	}


	public static void Fire(ProjectileID ProjectileId, Player UsingPlayer)
	{
		int Firer = UsingPlayer.Id;
		Vector3 Position = UsingPlayer.ProjectileEmitter.GlobalTransform.origin;
		Vector3 Rotation = new Vector3(-UsingPlayer.IntendedLookVertical, UsingPlayer.LookHorizontal, 0);
		Vector3 Momentum = new Vector3(0, 0, Data[ProjectileId].InitialSpeed)
			.Rotated(new Vector3(1,0,0), Deg2Rad(Rotation.x))
			.Rotated(new Vector3(0,1,0), Deg2Rad(Rotation.y));
		string NameArg = System.Guid.NewGuid().ToString();

		Self.ServerPleaseFire(ProjectileId, Firer, Position, Rotation, Momentum, NameArg);
	}


	[Remote]
	private void ServerPleaseFire(ProjectileID ProjectileId, int Firer, Vector3 Position, Vector3 Rotation, Vector3 Momentum, string NameArg)
	{
		if(!Net.Work.IsNetworkServer())
		{
			RpcId(Net.ServerId, nameof(ServerPleaseFire), ProjectileId, Firer, Position, Rotation, Momentum, NameArg);
			return;
		}

		//Only fire on ourself, the entity messaging system will spawn it on any clients neading it
		ActualFire(ProjectileId, Firer, Position, Rotation, Momentum, NameArg);
	}


	[Remote]
	public void ActualFire(ProjectileID ProjectileId, int Firer, Vector3 Position, Vector3 Rotation, Vector3 Momentum, string NameArg)
	{
		var Instance = (IProjectile) Data[ProjectileId].Scene.Instance();
		Instance.ProjectileId = ProjectileId;
		Instance.FirerId = Firer;
		Instance.Translation = Position;
		Instance.RotationDegrees = Rotation;
		Instance.Momentum = Momentum;
		Instance.Name = NameArg;
		World.EntitiesRoot.AddChild((Node)Instance);
	}
}
