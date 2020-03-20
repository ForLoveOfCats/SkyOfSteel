using Godot;



public interface IProjectile : IEntity
{
	Projectiles.ProjectileID ProjectileId { get; set; }
	Vector3 RotationDegrees { get; set; }
	Vector3 Momentum { get; set; }

	int FirerId { get; set; }

	void ProjectileCollided(Vector3 CollisionPointPosition);
}


public class ProjectileCollision : Spatial
{
	#pragma warning disable 0649
	[Export] private string StartPointPath;
	[Export] private string EndPointPath;
	#pragma warning restore 0649

	IProjectile Parent;
	Spatial StartPoint;
	Spatial EndPoint;


	public override void _Ready()
	{
		Parent = GetParent<IProjectile>();
		Assert.ActualAssert(Parent != null);

		StartPoint = GetNode<Spatial>(StartPointPath);
		Assert.ActualAssert(StartPoint != null);

		EndPoint = GetNode<Spatial>(EndPointPath);
		Assert.ActualAssert(EndPoint != null);
	}


	public override void _PhysicsProcess(float Delta)
	{
		if(!Net.Work.IsNetworkServer())
			return;

		PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
		Godot.Collections.Dictionary Results = State.IntersectRay(
			StartPoint.GlobalTransform.origin,
			EndPoint.GlobalTransform.origin,
			new Godot.Collections.Array {
				Net.Players[Parent.FirerId].Plr.ValueOr(() => null)
			},
			1
		);
		if(Results.Count > 0)
			Parent.ProjectileCollided((Vector3)Results["position"]);
	}
}
