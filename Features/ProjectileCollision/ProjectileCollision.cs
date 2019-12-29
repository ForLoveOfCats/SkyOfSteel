using Godot;
using static System.Diagnostics.Debug;


public interface IProjectileCollision
{
	void ProjectileCollided(Vector3 CollisionPointPosition);
}


public class ProjectileCollision : Spatial
{
#pragma warning disable 0649
	[Export] private string StartPointPath;
	[Export] private string EndPointPath;
#pragma warning restore 0649

	IProjectileCollision Parent;
	Spatial StartPoint;
	Spatial EndPoint;


	public override void _Ready()
	{
		Parent = GetParent<IProjectileCollision>();
		Assert(Parent != null);

		StartPoint = GetNode<Spatial>(StartPointPath);
		Assert(StartPoint != null);

		EndPoint = GetNode<Spatial>(EndPointPath);
		Assert(EndPoint != null);
	}


	public override void _PhysicsProcess(float Delta)
	{
		PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
		Godot.Collections.Dictionary Results = State.IntersectRay(
			StartPoint.GlobalTransform.origin,
			EndPoint.GlobalTransform.origin,
			new Godot.Collections.Array() { Game.PossessedPlayer }, 2
		);
		if(Results.Count > 0)
			Parent.ProjectileCollided((Vector3)Results["position"]);
	}
}
