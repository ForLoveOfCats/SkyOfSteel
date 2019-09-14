using Godot;
using System.Collections.Generic;



public interface IPipe : IInGrid
{
	PipeSystem System { get; set; }


	HashSet<IPipe> Friends { get; set; }


	void RecursiveAddFriendsToSystem();
}
