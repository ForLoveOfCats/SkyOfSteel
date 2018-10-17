using Godot;
using System;

public class Perform : Node
{
	public static void PlayerMove(int Id, Vector3 Position)
	{
		Events.Run(Events.TYPE.PLAYER_MOVE, new object[] {Id, Position});
	}
}
