using Godot;


class ShouldDo
{
	//All functions here check the server script for now as the client script
	  //is not loaded yet


	private static bool CheckFunctionServer(string FunctionName, object[] Args)
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


	private static bool CheckFunctionClient(string FunctionName, object[] Args)
	{
		try
		{
			return Scripting.ClientGmEngine.CallGlobalFunction<bool>(FunctionName, Args);
		}
		catch(System.InvalidOperationException)
		{
			return true;
		}
	}


	private static bool CheckFunctionBoth(string FunctionName, object[] Args)
	{
		//If either script returns false then return false otherwise if both
		  //return true then return true
		//Due to how || works, if the server script returns false then the
		  //client script never sees the event
		if(!CheckFunctionServer(FunctionName, Args) || !CheckFunctionClient(FunctionName, Args))
		{
			return false;
		}
		return true; 
	}


	public static bool LocalPlayerMove(Vector3 Position)
	{
		return CheckFunctionServer("_local_player_move", Scripting.ToJs(new object[] {Position}));
	}


	public static bool LocalPlayerRotate(float Rotation)
	{
		return CheckFunctionServer("_local_player_rotate", Scripting.ToJs(new object[] {Rotation}));
	}


	public static bool LocalPlayerYaw(float Rotation)
	{
		return CheckFunctionServer("_local_player_yaw", Scripting.ToJs(new object[] {Rotation}));
	}
}
