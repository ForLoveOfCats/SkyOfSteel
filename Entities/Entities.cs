using Godot;



public class Entities : Node
{
	public enum PURPOSE
	{
		CREATE,
		DESTROY,
		UPDATE,
	}


	public static Entities Self;
	private Entities()
	{
		Self = this;
	}


	[Remote]
	private void PleaseSendMeCreate(string Identifier)
	{
		if(!Net.Work.IsNetworkServer())
			return;

		Node Entity = World.EntitiesRoot.GetNodeOrNull(Identifier);
		if(Entity is null)
			return;
	}


	[Remote]
	private void Recieve(PURPOSE Purpose, string Identifier, object[] Args)
	{
		if(!Net.Work.IsNetworkServer() && Net.Work.GetRpcSenderId() != Net.ServerId)
		{
			Console.ThrowLog("Recieved entity message from another client");
			return;
		}

		switch(Purpose)
		{
			case PURPOSE.CREATE:
			{
				if(World.EntitiesRoot.HasNode(Identifier))
					return;

				return;
			}

			case PURPOSE.DESTROY:
			{
				Node Entity = World.EntitiesRoot.GetNodeOrNull(Identifier);
				if(Entity is null)
					return;

				return;
			}

			case PURPOSE.UPDATE:
			{
				Node Entity = World.EntitiesRoot.GetNodeOrNull(Identifier);
				if(Entity is null)
				{
					RpcId(Net.ServerId, nameof(PleaseSendMeCreate), Identifier);
					return;
				}

				return;
			}
		}
	}
}
