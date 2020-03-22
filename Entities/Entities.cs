using Godot;
using System;
using System.Collections.Generic;



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
		GD.Print("Received PleaseSendMeCreate");

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


	public static void SendCreate(int Receiver, IEntity Entity)
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception($"Cannot run {nameof(SendCreate)} on client");

		if(((Node)Entity).IsQueuedForDeletion())
			return;

		switch(Entity)
		{
			case IProjectile Projectile:
			{
				Projectiles.Self.RpcId(
					Receiver,
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

			case DroppedItem Item:
			{
				World.Self.RpcId(
					Receiver,
					nameof(World.DropOrUpdateItem),
					Item.Type,
					Item.Translation,
					Item.RotationDegrees.y,
					Item.Momentum,
					Item.Name
				);
				return;
			}

			case Tile Branch:
			{
				World.Self.RpcId(
					Receiver,
					nameof(World.PlaceWithName),
					Branch.ItemId,
					Branch.Translation,
					Branch.RotationDegrees,
					Branch.OwnerId,
					Branch.Name
				);
				return;
			}

			case Player Plr:
			{
				Game.Self.RpcId(
					Receiver,
					nameof(Game.NetSpawnPlayer),
					Plr.Id
				);
				return;
			}
		}
	}


	//Checks if the entity should be phased out
	//On the client a phase out is to be freed but not "destroyed"
	//On the server a phase out is to be made invisible
	public static void AsServerMaybePhaseOut(IEntity Entity)
	{
		foreach(KeyValuePair<int, Net.PlayerData> KV in Net.Players)
		{
			int Receiver = KV.Key;
			KV.Value.Plr.MatchSome(
				(Plr) =>
				{
					int ChunkRenderDistance = Game.ChunkRenderDistance;
					if(Receiver != Net.ServerId)
						ChunkRenderDistance = World.ChunkRenderDistances[Receiver];

					float Distance = World.GetChunkPos(Entity.Translation).DistanceTo(Plr.Translation.Flattened());
					if(Distance > ChunkRenderDistance*World.PlatformSize*9)
					{
						if(Receiver == Net.ServerId)
							Entity.Visible = false;
						else
							Entities.Self.RpcUnreliableId(Receiver, nameof(Entities.ReceivePhaseOut), Entity.Name);
					}
				}
			);
		}
	}


	[Remote]
	private void ReceivePhaseOut(string Identifier)
	{
		GD.Print("Received phase out");

		Node Entity = World.EntitiesRoot.GetNodeOrNull(Identifier);
		if(Entity is null)
			return;

		Assert.ActualAssert(Entity is IEntity);
		((IEntity)Entity).PhaseOut();
	}


	[Remote]
	public void PleaseDestroyMe(string Identifier, params object[] Args)
	{
		if(Net.Work.IsNetworkServer())
		{
			SendDestroy(Identifier, Args);
			ReceiveDestroy(Identifier, Args);
		}
		else
			Self.RpcId(Net.ServerId, nameof(PleaseDestroyMe), Identifier, Args);
	}


	public static void SendDestroy(string Identifier, params object[] Args)
	{
		if(Net.Work.IsNetworkServer())
			Net.SteelRpc(Self, nameof(ReceiveDestroy), Identifier, Args);
		else
			throw new Exception($"Cannot run {nameof(SendDestroy)} on client");
	}


	[Remote]
	private void ReceiveDestroy(string Identifier, params object[] Args)
	{
		GD.Print("Received destroy");

		Node Entity = World.EntitiesRoot.GetNodeOrNull(Identifier);
		if(Entity is null)
			return;

		Assert.ActualAssert(Entity is IEntity);
		((IEntity)Entity).Destroy(Args);
	}


	public static void ClientSendUpdate(string Identifier, params object[] Args)
	{
		if(Net.Work.IsNetworkServer())
			Self.ReceiveClientSendUpdate(Net.Work.GetNetworkUniqueId(), Identifier, Args);
		else
			Self.RpcUnreliableId(Net.ServerId, nameof(ReceiveClientSendUpdate), Net.Work.GetNetworkUniqueId(), Identifier, Args);
	}


	[Remote]
	private void ReceiveClientSendUpdate(int ClientId, string Identifier, params object[] Args)
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception($"Cannot run {nameof(SendUpdate)} on client");

		IEntity Entity = World.EntitiesRoot.GetNode<IEntity>(Identifier);

		foreach(int Receiver in Net.Players.Keys)
		{
			if(Receiver == ClientId)
				continue;
			else if(Receiver == Net.ServerId)
			{
				Self.ReceiveUpdate(Identifier, Args);
				continue;
			}

			Net.Players[Receiver].Plr.MatchSome(
				(Plr) =>
				{
					float Distance = World.GetChunkPos(Entity.Translation).DistanceTo(Plr.Translation.Flattened());
					if(Distance <= World.ChunkRenderDistances[Receiver] * (World.PlatformSize * 9))
						Self.RpcUnreliableId(Receiver, nameof(ReceiveUpdate), Identifier, Args);
				}
			);
		}
	}


	public static void SendUpdate(string Identifier, params object[] Args)
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception($"Cannot run {nameof(SendUpdate)} on client");

		IEntity Entity = World.EntitiesRoot.GetNode<IEntity>(Identifier);

		foreach(int Receiver in Net.Players.Keys)
		{
			if(Receiver == Net.ServerId)
				continue;

			Net.Players[Receiver].Plr.MatchSome(
				(Plr) =>
				{
					float Distance = World.GetChunkPos(Entity.Translation).DistanceTo(Plr.Translation.Flattened());
					if(Distance <= World.ChunkRenderDistances[Receiver] * (World.PlatformSize * 9))
						Self.RpcUnreliableId(Receiver, nameof(ReceiveUpdate), Identifier, Args);
				}
			);
		}
	}


	[Remote]
	private void ReceiveUpdate(string Identifier, params object[] Args)
	{
		GD.Print("Received update");

		Node Entity = World.EntitiesRoot.GetNodeOrNull(Identifier);
		if(Entity is null)
		{
			RpcId(Net.ServerId, nameof(PleaseSendMeCreate), Identifier);
			return;
		}

		Assert.ActualAssert(Entity is IEntity);
		((IEntity)Entity).Update(Args);
	}


	public static void SendInventory(IEntity Entity)
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception($"Cannot run {nameof(SendInventory)} on client");

		if(Entity is IHasInventory HasInventory)
		{
			foreach(int Receiver in Net.Players.Keys)
			{
				if(Receiver == Net.Work.GetNetworkUniqueId())
					continue;

				Net.Players[Receiver].Plr.MatchSome(
					(Plr) =>
					{
						float Distance = World.GetChunkPos(Entity.Translation).DistanceTo(Plr.Translation.Flattened());
						if(Distance <= World.ChunkRenderDistances[Receiver] * (World.PlatformSize * 9))
						{
							var Ids = new Items.ID[HasInventory.Inventory.Contents.Length];
							var Counts = new int[HasInventory.Inventory.Contents.Length];

							int Index = 0;
							foreach(Items.Instance Item in HasInventory.Inventory.Contents)
							{
								Ids[Index] = Item.Id;
								Counts[Index] = Item.Count;
								Index += 1;
							}

							Self.RpcUnreliableId(Receiver, nameof(ReceiveInventory), Entity.Name, Ids, Counts);
						}
					}
				);
			}
		}
		else
			Console.ThrowLog("Attempted to send the inventory of an entity without an inventory");
	}


	[Remote]
	private void ReceiveInventory(string Identifier, Items.ID[] Ids, int[] Counts)
	{
		GD.Print("Received inventory");

		Node Entity = World.EntitiesRoot.GetNodeOrNull(Identifier);
		if(Entity is null)
		{
			RpcId(Net.ServerId, nameof(PleaseSendMeCreate), Identifier);
			return;
		}

		Assert.ActualAssert(Entity is IEntity);
		if(Entity is IHasInventory HasInventory)
		{
			Assert.ActualAssert(Ids.Length == Counts.Length);
			Assert.ActualAssert(HasInventory.Inventory.Contents.Length == Ids.Length);

			int Index = 0;
			while(Index < Ids.Length)
			{
				if(Ids[Index] == Items.ID.NONE)
					HasInventory.Inventory.Contents[Index] = null;
				else
				{
					HasInventory.Inventory.Contents[Index].Id = Ids[Index];
					HasInventory.Inventory.Contents[Index].Count = Counts[Index];
				}
			}
		}
		else
			Console.ThrowLog("Received an inventory for an entity without an inventory");
	}
}
