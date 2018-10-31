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
}
