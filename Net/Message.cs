using Godot;
using System;


public class Message : Node
{
	public static void PlayerRequestPos(Vector3 Position)
	{
		Net.SendUnreliableMessage(Net.ServerId, Net.MESSAGE.PLAYER_REQUEST_POS, new object[] {Position});
	}


	public static void PlayerRequestRot(float Rotation)
	{
		Net.SendUnreliableMessage(Net.ServerId, Net.MESSAGE.PLAYER_REQUEST_ROT, new object[] {Rotation});
	}


	public static void ServerUpdatePlayerPos(int Peer, int Id, Vector3 Position)
	{
		Net.SendUnreliableMessage(Peer, Net.MESSAGE.PLAYER_UPDATE_POS, new object[] {Id, Position});
	}


	public static void ServerUpdatePlayerRot(int Peer, int Id, float Rotation)
	{
		Net.SendUnreliableMessage(Peer, Net.MESSAGE.PLAYER_UPDATE_ROT, new object[] {Id, Rotation});
	}


	public static void ServerUpdatePeerList(int Peer, int[] PeerList)
	{
		Net.SendMessage(Peer, Net.MESSAGE.PEERLIST_UPDATE, new object[] {PeerList});
	}


	public static void NetPlaceRequest(int OwnerId, Items.TYPE BranchType, Vector3 Position, Vector3 Rotation)
	{
		Net.SendMessage(Net.ServerId, Net.MESSAGE.PLACE_REQUEST, new object[] {OwnerId, BranchType, Position, Rotation});
	}

	public static void NetPlaceSync(Items.TYPE BranchType, Vector3 Position, Vector3 Rotation, int OwnerId, string Name)
	{
		foreach(int ClientId in Net.PeerList)
		{
			if(ClientId != 1)
			{
				Net.SendMessage(ClientId, Net.MESSAGE.PLACE_SYNC, new object[] {OwnerId, BranchType, Position, Rotation, Name});
			}
		}
	}
}
