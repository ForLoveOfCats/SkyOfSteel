using Godot;


public class Gamemode : Node
{
	public dynamic Self;

	public string LoadPath = null; //Fail early
	public string OwnName = null; //Fail early


	public virtual void OnUnload()
	{}


	public virtual void OnPlayerConnect(int PeerId)
	{}


	public virtual void OnPlayerDisconnect(int PeerId)
	{}

	public virtual void OnPlayerCollide(KinematicCollision Collision)
	{}

	public virtual bool ShouldPlayerMove(Vector3 Position)
	{
		return true;
	}


	public virtual bool ShouldMoveForward(float Sens)
	{
		return true;
	}


	public virtual bool ShouldMoveBackward(float Sens)
	{
		return true;
	}


	public virtual bool ShouldMoveRight(float Sens)
	{
		return true;
	}


	public virtual bool ShouldMoveLeft(float Sens)
	{
		return true;
	}


	public virtual bool ShouldJump()
	{
		return true;
	}


	public virtual bool ShouldWallKick()
	{
		return true;
	}


	public virtual bool ShouldCrouch()
	{
		return true;
	}


	public virtual bool ShouldThrowItem()
	{
		return true;
	}


	public virtual bool ShouldPickupItem(Items.ID Type)
	{
		return true;
	}


	public virtual bool ShouldToggleFly() //Only prevents ToggleFly called by keybinds, can be overrode by console
	{
		return true;
	}


	public virtual bool ShouldMovementReset()
	{
		return true;
	}


	public virtual bool ShouldPlayerRotate(float Rotation)
	{
		return true;
	}


	public virtual bool ShouldPlayerPitch(float Rotation)
	{
		return true;
	}


	public virtual bool ShouldSyncRemotePlayerPosition(int PeerId, Vector3 Position)
	{
		return true;
	}


	public virtual bool ShouldSyncRemotePlayerRotation(int PeerId, Vector3 Position)
	{
		return true;
	}


	public virtual bool ShouldPlaceStructure(Items.ID BranchType, Vector3 Position, Vector3 Rotation)
	{
		return true;
	}


	public virtual bool ShouldRemoveStructure(Items.ID BranchType, Vector3 Position, Vector3 Rotation, int OwnerId)
	{
		return true;
	}
}
