using System;
using Godot;


class ShouldDo
{
	private static bool CheckFunction(string FunctionName, params object[] Args)
	{
		if(Game.Self.GetTree().NetworkPeer == null || !Game.Self.GetTree().IsNetworkServer() || Scripting.GamemodeName == null)
		{
			return true;
			//If we are not the server, network is not ready, or no gamemode is loaded
			  //then just return true for any event so that nothing is modified
		}

		object Function = null;
		// Scripting.GmScope.TryGetVariable(FunctionName, out Function);
		if(Function != null /*&& Function is PythonFunction*/)
		{
			try
			{
				object Returned = true;//Scripting.GmEngine.Operations.Invoke(Function, Args);
				if(Returned is bool)
				{
					return (bool)Returned;
				}
				else
				{
					Console.ThrowLog($"Gamemode event '{FunctionName}' did not return a bool");
					Scripting.UnloadGameMode();
				}
			}
			catch(Exception Err)
			{
				// ExceptionOperations EO = Scripting.GmEngine.GetService<ExceptionOperations>();
				// Console.ThrowLog(EO.FormatException(Err));
				Scripting.UnloadGameMode();
			}
		}
		return true;
	}


	public static bool LocalPlayerMove(Vector3 Position)
	{
		return CheckFunction("_local_player_move", Position);
	}


	public static bool LocalPlayerForward(float Sens)
	{
		return CheckFunction("_local_player_forward", Sens);
	}


	public static bool LocalPlayerBackward(float Sens)
	{
		return CheckFunction("_local_player_backward", Sens);
	}


	public static bool LocalPlayerRight(float Sens)
	{
		return CheckFunction("_local_player_right", Sens);
	}


	public static bool LocalPlayerLeft(float Sens)
	{
		return CheckFunction("_local_player_left", Sens);
	}


	public static bool LocalPlayerJump()
	{
		return CheckFunction("_local_player_jump");
	}


	public static bool LocalPlayerRotate(float Rotation)
	{
		return CheckFunction("_local_player_rotate", Rotation);
	}


	public static bool LocalPlayerPitch(float Rotation)
	{
		return CheckFunction("_local_player_pitch", Rotation);
	}


	public static bool RemotePlayerMove(int PlayerId, Vector3 Position)
	{
		return CheckFunction("_remote_player_move", PlayerId, Position);
	}


	public static bool RemotePlayerRotate(int PlayerId, Vector3 Rotation)
	{
		return CheckFunction("_remote_player_rotate", PlayerId, Rotation);
	}


	public static bool StructurePlace(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId)
	{
		return CheckFunction("_structure_place", BranchType.ToString(), Position, Rotation, OwnerId);
	}


	public static bool StructureRemove(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId)
	{
		return CheckFunction("_structure_remove", BranchType.ToString(), Position, Rotation, OwnerId);
	}
}
