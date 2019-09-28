using Godot;
using static Godot.Mathf;
using static SteelMath;
using System.Collections.Generic;



public class Pipe : PipeCoreLogic
{
	private bool InitallyFilledFriends = false;

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
		System = new PipeSystem(this);
		Friends = new HashSet<PipeCoreLogic>();

		Position1 = GetNode<Spatial>("Positions/Position1");
		Position2 = GetNode<Spatial>("Positions/Position2");

		FirstEndMesh = GetNode<MeshInstance>("FirstEndMesh");
		FirstEndCollision = GetNode<CollisionShape>("FirstEndCollision");
		FirstOpenEnd = GetNode<StaticBody>("FirstOpenEnd");
		SecondEndMesh = GetNode<MeshInstance>("SecondEndMesh");
		SecondEndCollision = GetNode<CollisionShape>("SecondEndCollision");
		SecondOpenEnd = GetNode<StaticBody>("SecondOpenEnd");

		CallDeferred(nameof(GridUpdate));
	}


	public override void GridUpdate()
	{
		HashSet<PipeCoreLogic> OriginalFriends = Friends;
		Friends = new HashSet<PipeCoreLogic>();

		PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
		Godot.Collections.Dictionary Results;
		Results = State.IntersectRay(Translation, Position1.GlobalTransform.origin, new Godot.Collections.Array() { this, FirstOpenEnd, Game.PossessedPlayer }, 2|4);
		if(Results.Count > 0 && Results["collider"] is OpenEnd)
		{
			FirstEndMesh.Show();
			FirstEndCollision.Disabled = false;
			System.Consume(((OpenEnd)Results["collider"]).Parent.System);
			Friends.Add(((OpenEnd)Results["collider"]).Parent);
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
			System.Consume(((OpenEnd)Results["collider"]).Parent.System);
			Friends.Add(((OpenEnd)Results["collider"]).Parent);
		}
		else
		{
			SecondEndMesh.Hide();
			SecondEndCollision.Disabled = true;
		}

		if(InitallyFilledFriends && !Friends.SetEquals(OriginalFriends))
		{
			System = new PipeSystem(this);
			RecursiveAddFriendsToSystem();
		}
		InitallyFilledFriends = true;
	}
}
