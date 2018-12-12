using Godot;


public class Structure : StaticBody
{
	public Items.TYPE Type = Items.TYPE.ERROR;
	public int OwnerId = 0;


	public void Remove()
	{
		Rpc("NetRemove");
		QueueFree();
	}


	[Remote]
	public void NetRemove()
	{
		QueueFree();
	}
}
