using Godot;
using System;


public class Message : Node
{
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


	public static void NetRemoveRequest(string Name)
	{
		Net.SendMessage(Net.ServerId, Net.MESSAGE.REMOVE_REQUEST, new object[] {Name});
	}


	public static void NetRemoveSync(string Name)
	{
		foreach(int ClientId in Net.PeerList)
		{
			if(ClientId != 1)
			{
				Net.SendMessage(ClientId, Net.MESSAGE.REMOVE_SYNC, new object[] {Name});
			}
		}
	}
}
