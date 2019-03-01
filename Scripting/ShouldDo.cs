using Godot;


class ShouldDo
{
	//All functions here check the server script for now as the client script
	  //is not loaded yet


	private static bool CheckFunctionServer(string FunctionName, object[] Args)
	{
		/*if(Game.Self.GetTree().NetworkPeer == null || !Game.Self.GetTree().IsNetworkServer())
		{
			return true;
			//If we are not the server or the network is not ready then just return
			  //true for any event so that nothing is modified or affected
		}

		try
		{
			return Scripting.ServerGmEngine.CallGlobalFunction<bool>(FunctionName, Args);
		}
		catch(System.InvalidOperationException)
		{
			return true;
			//Also catches when the script is not running
		}*/
		return true;
	}


	private static bool CheckFunctionClient(string FunctionName, object[] Args)
	{
		/*try
		{
			return Scripting.ClientGmEngine.CallGlobalFunction<bool>(FunctionName, Args);
		}
		catch(System.InvalidOperationException)
		{
			return true;
			//Also catches when the script is not running
		}*/
		return true;
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
		return CheckFunctionClient("_local_player_move", Scripting.ToJs(new object[] {Position}));
	}


	public static bool LocalPlayerForward(double Sens)
	{
		return CheckFunctionClient("_local_player_forward", Scripting.ToJs(new object[] {Sens}));
	}


	public static bool LocalPlayerBackward(double Sens)
	{
		return CheckFunctionClient("_local_player_backward", Scripting.ToJs(new object[] {Sens}));
	}


	public static bool LocalPlayerRight(double Sens)
	{
		return CheckFunctionClient("_local_player_right", Scripting.ToJs(new object[] {Sens}));
	}


	public static bool LocalPlayerLeft(double Sens)
	{
		return CheckFunctionClient("_local_player_left", Scripting.ToJs(new object[] {Sens}));
	}


	public static bool LocalPlayerJump()
	{
		return CheckFunctionClient("_local_player_jump", new object[] {});
	}


	public static bool LocalPlayerRotate(float Rotation)
	{
		return CheckFunctionClient("_local_player_rotate", Scripting.ToJs(new object[] {Rotation}));
	}


	public static bool LocalPlayerPitch(float Rotation)
	{
		return CheckFunctionClient("_local_player_pitch", Scripting.ToJs(new object[] {Rotation}));
	}


	public static bool RemotePlayerMove(int PlayerId, Vector3 Position)
	{
		return CheckFunctionBoth("_remote_player_move", Scripting.ToJs(new object[] {PlayerId, Position}));
	}


	public static bool RemotePlayerRotate(int PlayerId, Vector3 Rotation)
	{
		return CheckFunctionBoth("_remote_player_rotate", Scripting.ToJs(new object[] {PlayerId, Rotation}));
	}


	public static bool StructurePlace(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId)
	{
		return CheckFunctionBoth("_structure_place", Scripting.ToJs(new object[] {BranchType.ToString(), Position, Rotation, OwnerId}));
	}


	public static bool StructureRemove(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId)
	{
		return CheckFunctionBoth("_structure_remove", Scripting.ToJs(new object[] {BranchType.ToString(), Position, Rotation, OwnerId}));
	}
}
