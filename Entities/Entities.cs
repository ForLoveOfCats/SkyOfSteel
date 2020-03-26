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
		SendCreateTo(Requester, (IEntity)Entity);
	}


	public static void SendCreate(IEntity Entity)
	{
		foreach(KeyValuePair<int, Net.PlayerData> KV in Net.Players)
		{
			int Receiver = KV.Key;
			if(Receiver == Net.Work.GetNetworkUniqueId())
				continue;

			KV.Value.Plr.MatchSome(
				(Plr) =>
				{
					int ChunkRenderDistance = World.ChunkRenderDistances[Receiver];
					var EntityChunk = World.GetChunkTuple(Entity.Translation);
					if(World.ChunkWithinDistanceFrom(EntityChunk, ChunkRenderDistance, Plr.Translation))
						SendCreateTo(Receiver, Entity);
				}
			);
		}
	}


	public static void SendCreateTo(int Receiver, IEntity Entity)
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception($"Cannot run {nameof(SendCreateTo)} on client");

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


	public static void MovedTick(IEntity Entity, Tuple<int, int> OriginalChunkTuple)
	{
		var ChunkTuple = World.GetChunkTuple(Entity.Translation);

		if(!ChunkTuple.Equals(OriginalChunkTuple))
		{
			World.RemoveEntityFromChunk(Entity);
			World.AddEntityToChunk(Entity);
		}
	}


	//Checks if the entity should be phased out
	//On the client a phase out frees the entity but does not "destroy" it
	//On the server a phase out makes the entity invisible
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

					var EntityChunk = World.GetChunkTuple(Entity.Translation);
					if(World.ChunkWithinDistanceFrom(EntityChunk, ChunkRenderDistance, Plr.Translation))
					{
						if(Receiver == Net.ServerId)
							Entity.Visible = true;
					}
					else
					{
						if(Receiver == Net.ServerId)
							Entity.Visible = false;
						else
							Entities.Self.RpcId(Receiver, nameof(Entities.ReceivePhaseOut), Entity.Name);
					}
				}
			);
		}
	}


	[Remote]
	private void ReceivePhaseOut(string Identifier)
	{
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
					var EntityChunk = World.GetChunkTuple(Entity.Translation);
					var ChunkDistance = World.ChunkRenderDistances[Receiver];
					if(World.ChunkWithinDistanceFrom(EntityChunk, ChunkDistance, Plr.Translation))
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
					var EntityChunk = World.GetChunkTuple(Entity.Translation);
					if(World.ChunkWithinDistanceFrom(EntityChunk, World.ChunkRenderDistances[Receiver], Plr.Translation))
						Self.RpcUnreliableId(Receiver, nameof(ReceiveUpdate), Identifier, Args);
				}
			);
		}
	}


	[Remote]
	private void ReceiveUpdate(string Identifier, params object[] Args)
	{
		Node Entity = World.EntitiesRoot.GetNodeOrNull(Identifier);
		if(Entity is null)
		{
			RpcId(Net.ServerId, nameof(PleaseSendMeCreate), Identifier);
			return;
		}

		Assert.ActualAssert(Entity is IEntity);
		((IEntity)Entity).Update(Args);
	}


	public static void SendInventory(IHasInventory HasInventory)
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception($"Cannot run {nameof(SendInventory)} on client");

		foreach(int Receiver in Net.Players.Keys)
		{
			if(Receiver == Net.Work.GetNetworkUniqueId())
				continue;

			Net.Players[Receiver].Plr.MatchSome(
				(Plr) =>
				{
					var EntityChunk = World.GetChunkTuple(HasInventory.Translation);
					if(World.ChunkWithinDistanceFrom(EntityChunk, World.ChunkRenderDistances[Receiver], Plr.Translation))
					{
						var Ids = new Items.ID[HasInventory.Inventory.SlotCount];
						var Counts = new int[HasInventory.Inventory.SlotCount];

						int Index = 0;
						while(Index < HasInventory.Inventory.SlotCount)
						{
							Items.Instance Item = HasInventory.Inventory.Contents[Index];

							if(Item is null)
							{
								Ids[Index] = Items.ID.NONE;
								Counts[Index] = 0;
								Index += 1;
								continue;
							}

							Ids[Index] = Item.Id;
							Counts[Index] = Item.Count;
							Index += 1;
						}

						Self.RpcUnreliableId(Receiver, nameof(ReceiveInventory), HasInventory.Name, Ids, Counts);
					}
				}
			);
		}
	}


	public static void SendInventoryTo(IHasInventory HasInventory, int Receiver)
	{
		var Ids = new Items.ID[HasInventory.Inventory.SlotCount];
		var Counts = new int[HasInventory.Inventory.SlotCount];

		int Index = 0;
		while(Index < HasInventory.Inventory.SlotCount)
		{
			Items.Instance Item = HasInventory.Inventory.Contents[Index];

			if(Item is null)
			{
				Ids[Index] = Items.ID.NONE;
				Counts[Index] = 0;
				Index += 1;
				continue;
			}

			Ids[Index] = Item.Id;
			Counts[Index] = Item.Count;
			Index += 1;
		}

		Self.RpcUnreliableId(Receiver, nameof(ReceiveInventory), HasInventory.Name, Ids, Counts);
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
			Assert.ActualAssert(HasInventory.Inventory.SlotCount == Ids.Length);

			int Index = 0;
			while(Index < Ids.Length)
			{
				if(Ids[Index] == Items.ID.NONE)
					HasInventory.Inventory.Contents[Index] = null;
				else
				{
					var Item = new Items.Instance(Ids[Index]) {
						Count = Counts[Index]
					};
					HasInventory.Inventory.Contents[Index] = Item;
				}

				Index += 1;
			}
		}
		else
			Console.ThrowLog("Received an inventory for an entity without an inventory");
	}


	public static void SendPush(IPushable Pushable, Vector3 Push)
	{
		foreach(KeyValuePair<int, Net.PlayerData> KV in Net.Players)
		{
			int Receiver = KV.Key;
			if(Receiver == Net.Work.GetNetworkUniqueId())
				continue;

			KV.Value.Plr.MatchSome(
				(Plr) =>
				{
					int ChunkRenderDistance = World.ChunkRenderDistances[Receiver];
					var PushableChunk = World.GetChunkTuple(Pushable.Translation);
					if(World.ChunkWithinDistanceFrom(PushableChunk, ChunkRenderDistance, Plr.Translation))
						Self.RpcId(Receiver, nameof(ReceivePush), Pushable.Name, Push);
				}
			);
		}
	}


	[Remote]
	private void ReceivePush(string Identifier, Vector3 Push)
	{
		GD.Print("Received push");

		Node Entity = World.EntitiesRoot.GetNodeOrNull(Identifier);
		if(Entity is null)
		{
			RpcId(Net.ServerId, nameof(PleaseSendMeCreate), Identifier);
			return;
		}

		if(Entity is IPushable Pushable)
			Pushable.ApplyPush(Push);
		else
			Console.ThrowLog("Received a push message for an unpushable entity");
	}
}
