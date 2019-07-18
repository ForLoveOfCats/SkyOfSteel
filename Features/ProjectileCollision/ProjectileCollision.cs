using Godot;
using static System.Diagnostics.Debug;


public interface IProjectileCollision
{
	void ProjectileCollided(Vector3 CollisionPointPosition);
}


public class ProjectileCollision : Spatial
{
	[Export] string StartPointPath;
	[Export] string EndPointPath;

	Spatial Parent = null;
	Spatial StartPoint = null;
	Spatial EndPoint = null;


	public override void _Ready()
	{
		Parent = GetParent<Spatial>();
		Assert(Parent != null);
		Assert(Parent is IProjectileCollision);

		StartPoint = GetNode<Spatial>(StartPointPath);
		Assert(StartPoint != null);

		EndPoint = GetNode<Spatial>(EndPointPath);
		Assert(EndPoint != null);
	}


	public override void _PhysicsProcess(float Delta)
	{
		PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
		Godot.Collections.Dictionary Results = State.IntersectRay(StartPoint.GetGlobalTransform().origin,
		                                                          EndPoint.GetGlobalTransform().origin, null, 1);
		if(Results.Count > 0)
			(Parent as IProjectileCollision).ProjectileCollided((Vector3)Results["position"]);
	}
}
