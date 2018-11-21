using Godot;


class Filter
{
	public enum Type {PLAYER_MOVE};
	public enum Invoker {CLIENT, SERVER};


	public static void PlayerMove(Invoker InvokerArg, ref Vector3 Position)
	{
	}
}
