using Godot;
using System;

public class Perform : Node
{
	public static void LocalPlayerMove(Vector3 Position)
	{
		Events.Run(Events.TYPE.LOCAL_PLAYER_MOVE, new object[] {Position});
	}


	public static void LocalPlayerRotate(float Rotation)
	{
		Events.Run(Events.TYPE.LOCAL_PLAYER_ROT, new object[] {Rotation});
	}
}
