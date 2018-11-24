using Godot;


class ShouldDo
{
	private static bool CheckFunction(string FunctionName, object[] Args)
	{
		try
		{
			return Scripting.ServerGmEngine.CallGlobalFunction<bool>(FunctionName, Args);
		}
		catch(System.InvalidOperationException)
		{
			return true;
		}
	}


	public static bool LocalPlayerMove(Vector3 Position)
	{
		return CheckFunction("_local_player_move", Scripting.ToJs(new object[] {Position}));
	}
}
