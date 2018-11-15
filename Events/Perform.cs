using Godot;
using System;


public class Perform : Node
{
	public static void LocalPlayerMove(Events.INVOKER Invoker, Vector3 Position)
	{
		Events.Run(new EventObject(Invoker, Events.TYPE.LOCAL_PLAYER_MOVE, new object[] {Position}));
	}


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
}
