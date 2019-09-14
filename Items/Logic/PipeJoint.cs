using Godot;
using static Godot.Mathf;
using static SteelMath;
using System.Collections.Generic;



public class PipeJoint : Tile, IPipe
{
	public PipeSystem System { get; set; }
	public HashSet<IPipe> Friends { get; set; }
	private bool InitiallyFilledFriends = false;

	Spatial Position1;
	Spatial Position2;
	Spatial Position3;
	Spatial Position4;
	Spatial Position5;
	Spatial Position6;

	MeshInstance FirstEndMesh;
	CollisionShape FirstEndCollision;
	StaticBody FirstOpenEnd;

	MeshInstance SecondEndMesh;
	CollisionShape SecondEndCollision;
	StaticBody SecondOpenEnd;

	MeshInstance ThirdEndMesh;
	CollisionShape ThirdEndCollision;
	StaticBody ThirdOpenEnd;

	MeshInstance ForthEndMesh;
	CollisionShape ForthEndCollision;
	StaticBody ForthOpenEnd;

	MeshInstance FifthEndMesh;
	CollisionShape FifthEndCollision;
	StaticBody FifthOpenEnd;

	MeshInstance SixthEndMesh;
	CollisionShape SixthEndCollision;
	StaticBody SixthOpenEnd;

	public override void _Ready()
	{
		System = new PipeSystem(this);
		Friends = new HashSet<IPipe>();

		Position1 = GetNode<Spatial>("Positions/Position1");
		Position2 = GetNode<Spatial>("Positions/Position2");
		Position3 = GetNode<Spatial>("Positions/Position3");
		Position4 = GetNode<Spatial>("Positions/Position4");
		Position5 = GetNode<Spatial>("Positions/Position5");
		Position6 = GetNode<Spatial>("Positions/Position6");

		FirstEndMesh = GetNode<MeshInstance>("FirstEndMesh");
		FirstEndCollision = GetNode<CollisionShape>("FirstEndCollision");
		FirstOpenEnd = GetNode<StaticBody>("FirstOpenEnd");

		SecondEndMesh = GetNode<MeshInstance>("SecondEndMesh");
		SecondEndCollision = GetNode<CollisionShape>("SecondEndCollision");
		SecondOpenEnd = GetNode<StaticBody>("SecondOpenEnd");

		ThirdEndMesh = GetNode<MeshInstance>("ThirdEndMesh");
		ThirdEndCollision = GetNode<CollisionShape>("ThirdEndCollision");
		ThirdOpenEnd = GetNode<StaticBody>("ThirdOpenEnd");

		ForthEndMesh = GetNode<MeshInstance>("ForthEndMesh");
		ForthEndCollision = GetNode<CollisionShape>("ForthEndCollision");
		ForthOpenEnd = GetNode<StaticBody>("ForthOpenEnd");

		FifthEndMesh = GetNode<MeshInstance>("FifthEndMesh");
		FifthEndCollision = GetNode<CollisionShape>("FifthEndCollision");
		FifthOpenEnd = GetNode<StaticBody>("FifthOpenEnd");

		SixthEndMesh = GetNode<MeshInstance>("SixthEndMesh");
		SixthEndCollision = GetNode<CollisionShape>("SixthEndCollision");
		SixthOpenEnd = GetNode<StaticBody>("SixthOpenEnd");

		CallDeferred(nameof(GridUpdate));
	}


	public override void GridUpdate()
	{
		HashSet<IPipe> OriginalFriends = Friends;
		Friends = new HashSet<IPipe>();

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

		Results = State.IntersectRay(Translation, Position3.GlobalTransform.origin, new Godot.Collections.Array() { this, ThirdOpenEnd, Game.PossessedPlayer }, 2|4);
		if(Results.Count > 0 && Results["collider"] is OpenEnd)
		{
			ThirdEndMesh.Show();
			ThirdEndCollision.Disabled = false;
			System.Consume(((OpenEnd)Results["collider"]).Parent.System);
			Friends.Add(((OpenEnd)Results["collider"]).Parent);
		}
		else
		{
			ThirdEndMesh.Hide();
			ThirdEndCollision.Disabled = true;
		}

		Results = State.IntersectRay(Translation, Position4.GlobalTransform.origin, new Godot.Collections.Array() { this, ForthOpenEnd, Game.PossessedPlayer }, 2|4);
		if(Results.Count > 0 && Results["collider"] is OpenEnd)
		{
			ForthEndMesh.Show();
			ForthEndCollision.Disabled = false;
			System.Consume(((OpenEnd)Results["collider"]).Parent.System);
			Friends.Add(((OpenEnd)Results["collider"]).Parent);
		}
		else
		{
			ForthEndMesh.Hide();
			ForthEndCollision.Disabled = true;
		}

		Results = State.IntersectRay(Translation, Position5.GlobalTransform.origin, new Godot.Collections.Array() { this, FifthOpenEnd, Game.PossessedPlayer }, 2|4);
		if(Results.Count > 0 && Results["collider"] is OpenEnd)
		{
			FifthEndMesh.Show();
			FifthEndCollision.Disabled = false;
			System.Consume(((OpenEnd)Results["collider"]).Parent.System);
			Friends.Add(((OpenEnd)Results["collider"]).Parent);
		}
		else
		{
			FifthEndMesh.Hide();
			FifthEndCollision.Disabled = true;
		}

		Results = State.IntersectRay(Translation, Position6.GlobalTransform.origin, new Godot.Collections.Array() { this, SixthOpenEnd, Game.PossessedPlayer }, 2|4);
		if(Results.Count > 0 && Results["collider"] is OpenEnd)
		{
			SixthEndMesh.Show();
			SixthEndCollision.Disabled = false;
			System.Consume(((OpenEnd)Results["collider"]).Parent.System);
			Friends.Add(((OpenEnd)Results["collider"]).Parent);
		}
		else
		{
			SixthEndMesh.Hide();
			SixthEndCollision.Disabled = true;
		}

		if(InitiallyFilledFriends && !Friends.SetEquals(OriginalFriends))
		{
			System = new PipeSystem(this);
			RecursiveAddFriendsToSystem();
		}
		InitiallyFilledFriends = true;
	}


	public void RecursiveAddFriendsToSystem()
	{
		foreach(IPipe Friend in Friends)
		{
			if(Friend.System == System)
				continue;

			System.Pipes.Add(Friend);
			Friend.System = System;
			Friend.RecursiveAddFriendsToSystem();
		}
	}


	public override void OnRemove()
	{
		List<PipeSystem> JustCreated = new List<PipeSystem>();
		foreach(IPipe Friend in Friends)
		{
			Friend.Friends.Remove(this);

			if(JustCreated.Contains(Friend.System))
				continue;

			PipeSystem NewSystem = new PipeSystem(Friend);
			JustCreated.Add(NewSystem);
			Friend.System = NewSystem;
			Friend.RecursiveAddFriendsToSystem();
		}
	}
}
