using Godot;
using static Godot.Mathf;
using static SteelMath;



public class Pipe : Tile
{
	Spatial Position1;
	Spatial Position2;

	MeshInstance FirstEndMesh;
	CollisionShape FirstEndCollision;
	StaticBody FirstOpenEnd;
	MeshInstance SecondEndMesh;
	CollisionShape SecondEndCollision;
	StaticBody SecondOpenEnd;

	public override void _Ready()
	{
		Position1 = GetNode<Spatial>("Positions/Position1");
		Position2 = GetNode<Spatial>("Positions/Position2");

		FirstEndMesh = GetNode<MeshInstance>("FirstEndMesh");
		FirstEndCollision = GetNode<CollisionShape>("FirstEndCollision");
		FirstOpenEnd = GetNode<StaticBody>("FirstOpenEnd");
		SecondEndMesh = GetNode<MeshInstance>("SecondEndMesh");
		SecondEndCollision = GetNode<CollisionShape>("SecondEndCollision");
		SecondOpenEnd = GetNode<StaticBody>("SecondOpenEnd");

		GridUpdate();
	}


	public override void GridUpdate()
	{
		PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;

		Godot.Collections.Dictionary Results;
		Results = State.IntersectRay(Translation, Position1.GlobalTransform.origin, new Godot.Collections.Array() { this, FirstOpenEnd, Game.PossessedPlayer }, 2|4);
		if(Results.Count > 0 && Results["collider"] is OpenEnd)
		{
			FirstEndMesh.Show();
			FirstEndCollision.Disabled = false;
		}
		else
		{
			FirstEndMesh.Hide();
			FirstEndCollision.Disabled = true;
		}

		Results = State.IntersectRay(Translation, Position2.GlobalTransform.origin, new Godot.Collections.Array() { this, SecondOpenEnd, Game.PossessedPlayer }, 2|4);
		if(Results.Count > 0 && Results["collider"] is OpenEnd)
		{
			SecondEndMesh.Show();
			SecondEndCollision.Disabled = false;
		}
		else
		{
			SecondEndMesh.Hide();
			SecondEndCollision.Disabled = true;
		}
	}
}
