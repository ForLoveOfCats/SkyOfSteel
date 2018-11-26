using Godot;
using System;


public class Perform : Node
{
	public static void LocalPlayerRotate(Events.INVOKER Invoker, float Rotation)
	{
		Events.Run(new EventObject(Invoker, Events.TYPE.LOCAL_PLAYER_ROT, new object[] {Rotation}));
	}


	public static void RemotePlayerMove(Events.INVOKER Invoker, int Id, Vector3 Position)
	{
		Events.Run(new EventObject(Invoker, Events.TYPE.REMOTE_PLAYER_MOVE, new object[] {Id, Position}));
	}


	public static void RemotePlayerRotate(Events.INVOKER Invoker, int Id, float Rotation)
	{
		Events.Run(new EventObject(Invoker, Events.TYPE.REMOTE_PLAYER_ROT, new object[] {Id, Rotation}));
	}


	public static void PlaceRequest(Events.INVOKER Invoker, int OwnerId, Items.TYPE BranchType, Vector3 Position, Vector3 Rotation)
	{
		Events.Run(new EventObject(Invoker, Events.TYPE.PLACE_REQUEST, new object[] {OwnerId, BranchType, Position, Rotation}));
	}

	public static void Place(Events.INVOKER Invoker, int OwnerId, Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, string Name = null)
	{
		Events.Run(new EventObject(Invoker, Events.TYPE.PLACE, new object[] {OwnerId, BranchType, Position, Rotation, Name}));
	}

	public static void Remove(Events.INVOKER Invoker, string Name)
	{
		Events.Run(new EventObject(Invoker, Events.TYPE.REMOVE, new object[] {Name}));
	}
}
