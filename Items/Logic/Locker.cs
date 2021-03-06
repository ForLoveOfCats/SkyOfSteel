using Godot;
using static Godot.Mathf;
using static SteelMath;
using System.Collections.Generic;



public class Locker : PipeCoreLogic
{
	private bool InitiallyFilledFriends = false;

	Spatial Position1;

	MeshInstance OpenEndMesh;
	CollisionShape OpenEndCollision;
	StaticBody OpenEnd;

	public override void _Ready()
	{
		System = new PipeSystem(this);
		Friends = new HashSet<PipeCoreLogic>();

		Position1 = GetNode<Spatial>("Positions/Position1");
		OpenEndMesh = GetNode<MeshInstance>("OpenEndMesh");
		OpenEndCollision = GetNode<CollisionShape>("OpenEndCollision");
		OpenEnd = GetNode<StaticBody>("OpenEnd");

		CallDeferred(nameof(GridUpdate));
	}


	public override void GridUpdate()
	{
		HashSet<PipeCoreLogic> OriginalFriends = Friends;
		Friends = new HashSet<PipeCoreLogic>();

		PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
		Godot.Collections.Dictionary Results;
		Results = State.IntersectRay(Translation, Position1.GlobalTransform.origin, new Godot.Collections.Array() { this, OpenEnd, Game.PossessedPlayer }, 2|4);
		if(Results.Count > 0 && Results["collider"] is OpenEnd)
		{
			OpenEndMesh.Show();
			OpenEndCollision.Disabled = false;
			System.Consume(((OpenEnd)Results["collider"]).Parent.System);
			Friends.Add(((OpenEnd)Results["collider"]).Parent);
		}
		else
		{
			OpenEndMesh.Hide();
			OpenEndCollision.Disabled = true;
		}

		if(InitiallyFilledFriends && !Friends.SetEquals(OriginalFriends))
		{
			System = new PipeSystem(this);
			RecursiveAddFriendsToSystem();
		}
		InitiallyFilledFriends = true;
	}
}
