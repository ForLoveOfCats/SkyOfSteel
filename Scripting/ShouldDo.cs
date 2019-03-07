using Godot;
using IronPython;
using IronPython.Runtime;

class ShouldDo
{
	private static bool CheckFunction(string FunctionName, object[] Args)
	{
		/*if(Game.Self.GetTree().NetworkPeer == null || !Game.Self.GetTree().IsNetworkServer())
		{
			return true;
			//If we are not the server or the network is not ready then just return
			  //true for any event so that nothing is modified or affected
		}

		try
		{
			object Returned = Scripting.GmScope.GetVariable(FunctionName);
			if(Returned is PythonFunction)
			{
				Scripting.GmEngine.Operations.Invoke(Returned, Args);
			}
			// return Scripting.ServerGmEngine.CallGlobalFunction<bool>(FunctionName, Args);
		}
		catch(System.InvalidOperationException)
		{
			return true;
			//Also catches when the script is not running
		}*/
		return true;
	}


	public static bool LocalPlayerMove(Vector3 Position)
	{
		return CheckFunction("_local_player_move", Scripting.ToPy(new object[] {Position}));
	}


	public static bool LocalPlayerForward(double Sens)
	{
		return CheckFunction("_local_player_forward", Scripting.ToPy(new object[] {Sens}));
	}


	public static bool LocalPlayerBackward(double Sens)
	{
		return CheckFunction("_local_player_backward", Scripting.ToPy(new object[] {Sens}));
	}


	public static bool LocalPlayerRight(double Sens)
	{
		return CheckFunction("_local_player_right", Scripting.ToPy(new object[] {Sens}));
	}


	public static bool LocalPlayerLeft(double Sens)
	{
		return CheckFunction("_local_player_left", Scripting.ToPy(new object[] {Sens}));
	}


	public static bool LocalPlayerJump()
	{
		return CheckFunction("_local_player_jump", new object[] {});
	}


	public static bool LocalPlayerRotate(float Rotation)
	{
		return CheckFunction("_local_player_rotate", Scripting.ToPy(new object[] {Rotation}));
	}


	public static bool LocalPlayerPitch(float Rotation)
	{
		return CheckFunction("_local_player_pitch", Scripting.ToPy(new object[] {Rotation}));
	}


	public static bool RemotePlayerMove(int PlayerId, Vector3 Position)
	{
		return CheckFunction("_remote_player_move", Scripting.ToPy(new object[] {PlayerId, Position}));
	}


	public static bool RemotePlayerRotate(int PlayerId, Vector3 Rotation)
	{
		return CheckFunction("_remote_player_rotate", Scripting.ToPy(new object[] {PlayerId, Rotation}));
	}


	public static bool StructurePlace(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId)
	{
		return CheckFunction("_structure_place", Scripting.ToPy(new object[] {BranchType.ToString(), Position, Rotation, OwnerId}));
	}


	public static bool StructureRemove(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId)
	{
		return CheckFunction("_structure_remove", Scripting.ToPy(new object[] {BranchType.ToString(), Position, Rotation, OwnerId}));
	}
}
