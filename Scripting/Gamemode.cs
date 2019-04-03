using Godot;


public class Gamemode : Node
{
	public string LoadPath = null; //Fail early


	public virtual void OnUnload()
	{}


	public virtual bool ShouldPlayerMove(Vector3 Position)
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


	public virtual bool ShouldPlaceStructure(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId)
	{
		return true;
	}


	public virtual bool ShouldRemoveStructure(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId)
	{
		return true;
	}
}
