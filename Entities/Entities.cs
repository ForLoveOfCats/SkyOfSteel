using Godot;
using System;



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
		GD.Print("Recieved PleaseSendMeCreate");

		if(!Net.Work.IsNetworkServer())
			throw new Exception($"Cannot run {nameof(PleaseSendMeCreate)} on client");

		int Requester = Net.Work.GetRpcSenderId();
		if(Requester == 0)
			throw new Exception($"{nameof(PleaseSendMeCreate)} was run not as an RPC");

		Node Entity = World.EntitiesRoot.GetNodeOrNull(Identifier);
		if(Entity is null)
			return;

		Assert.ActualAssert(Entity is IEntity);
		SendCreate(Requester, (IEntity)Entity);
	}


	public static void SendCreate(int Reciever, IEntity Entity)
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception($"Cannot run {nameof(SendCreate)} on client");

		switch(Entity)
		{
			case IProjectile Projectile:
			{
				Projectiles.Self.RpcId(
					Reciever,
					nameof(Projectiles.ActualFire),
					Projectile.ProjectileId,
					Projectile.FirerId,
					Projectile.Translation,
					Projectile.RotationDegrees,
					Projectile.Momentum,
					Projectile.Name
				);
				return;
			}
		}
	}


	public static void SendDestroy(string Identifier, params object[] Args)
	{
		if(Net.Work.IsNetworkServer())
			Net.SteelRpc(Self, nameof(RecieveDestroy), Identifier, Args);
		else
			throw new Exception($"Cannot run {nameof(SendDestroy)} on client");
	}


	[Remote]
	private void RecieveDestroy(string Identifier, params object[] Args)
	{
		GD.Print("Recieved destroy");

		Node Entity = World.EntitiesRoot.GetNodeOrNull(Identifier);
		if(Entity is null)
			return;

		Assert.ActualAssert(Entity is IEntity);
		((IEntity)Entity).Destroy(Args);
	}


	public static void SendUpdate(string Identifier, params object[] Args)
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception($"Cannot run {nameof(SendUpdate)} on client");

		IEntity Entity = World.EntitiesRoot.GetNode<IEntity>(Identifier);

		foreach(int Reciever in Net.Players.Keys)
		{
			if(Reciever == Net.Work.GetNetworkUniqueId())
				continue;

			Net.Players[Reciever].Plr.MatchSome(
				(Plr) =>
				{
					float Distance = World.GetChunkPos(Entity.Translation).DistanceTo(Plr.Translation.Flattened());
					if(Distance <= World.ChunkLoadDistances[Reciever] * (World.PlatformSize * 9))
						Self.RpcUnreliableId(Reciever, nameof(RecieveUpdate), Identifier, Args);
				}
			);
		}
	}


	[Remote]
	private void RecieveUpdate(string Identifier, params object[] Args)
	{
		GD.Print("Recieved update");

		Node Entity = World.EntitiesRoot.GetNodeOrNull(Identifier);
		if(Entity is null)
		{
			RpcId(Net.ServerId, nameof(PleaseSendMeCreate), Identifier);
			return;
		}

		Assert.ActualAssert(Entity is IEntity);
		((IEntity)Entity).Update(Args);
	}
}
