using Godot;
using System;

public class Perform : Node
{
	public static void PlayerMove(int Id, Vector3 Position)
	{
		Events.Run(Events.TYPE.LOCALPLAYER_MOVE, new object[] {Id, Position});
	}


	public static void PlayerRotate(int Id, float Rotation)
	{
		Events.Run(Events.TYPE.LOCALPLAYER_ROT, new object[] {Id, Rotation});
	}
}
