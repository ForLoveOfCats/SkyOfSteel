using Godot;
using System.Collections.Generic;



public class PipeCoreLogic : Tile, IInGrid {
	public PipeSystem System { get; set; }
	public HashSet<PipeCoreLogic> Friends { get; set; }


	public void RecursiveAddFriendsToSystem() {
		foreach(PipeCoreLogic Friend in Friends) {
			if(Friend.System == System)
				continue;

			System.Pipes.Add(Friend);
			Friend.System = System;
			Friend.RecursiveAddFriendsToSystem();
		}
	}


	public override void OnRemove() {
		List<PipeSystem> JustCreated = new List<PipeSystem>();
		foreach(PipeCoreLogic Friend in Friends) {
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
