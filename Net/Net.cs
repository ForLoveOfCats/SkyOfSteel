using Godot;
using System;
using System.Collections.Generic;


public class Net : Node
{
	public enum MESSAGE {PLAYER_REQUEST_POS, PLAYER_REQUEST_ROT, PLAYER_UPDATE_POS, PLAYER_UPDATE_ROT, PEERLIST_UPDATE, PLACE_REQUEST};
	public static int ServerId = 1;

	private static int Port = 7777;
	private static string Ip;

	public static List<int> PeerList = new List<int>();

	public static Net Self;
	Net()
	{
		Self = this;
	}


	public override void _Ready()
	{
		GetTree().Connect("network_peer_connected", this, "_PlayerConnected");
		GetTree().Connect("network_peer_disconnected", this, "_PlayerDisconnected");
		GetTree().Connect("server_disconnected", this, "_ServerDisconnected");
	}


	public void _PlayerConnected(int Id)
	{
		if(Id == 1)
		{
			Console.Log("Connected to server at '" + Ip.ToString() + "'");
			Game.StartWorld();
			PeerList.Add(Self.GetTree().GetNetworkUniqueId());
			Game.SpawnPlayer(Self.GetTree().GetNetworkUniqueId(), true);
		}
		else
		{
			Console.Log("Player '" + Id.ToString() + "' connected");
		}

		Game.SpawnPlayer(Id, false);

		if(Self.GetTree().IsNetworkServer())
		{
			PeerList.Add(Id);
			foreach(int Peer in PeerList)
			{
				if(Peer != ServerId)
				{
					Message.ServerUpdatePeerList(Peer, PeerList.ToArray());
				}
			}
		}
	}


	public void _PlayerDisconnected(int Id)
	{
		Console.Log("Player '" + Id.ToString() + "' disconnected");
		Self.GetTree().GetRoot().GetNode("RuntimeRoot/SkyScene/" + Id.ToString()).QueueFree();

		if(Self.GetTree().IsNetworkServer())
		{
			PeerList.Remove(Id);
			foreach(int Peer in PeerList)
			{
				if(Peer != ServerId)
				{
					Message.ServerUpdatePeerList(Peer, PeerList.ToArray());
				}
			}
		}
	}


	public void _ServerDisconnected()
	{
		Console.Log("Lost connection to server at '" + Ip.ToString() + "'");
		GetTree().SetNetworkPeer(null);
		Game.CloseWorld();
		PeerList.Clear();
	}


	public static void Host()
	{
		PeerList.Clear();
		Game.StartWorld();

		NetworkedMultiplayerENet Peer = new NetworkedMultiplayerENet();
		Peer.CreateServer(Port, Game.MaxPlayers);
		Self.GetTree().SetNetworkPeer(Peer);
		Self.GetTree().SetMeta("network_peer", Peer);

		Console.Log("Started hosting on port '" + Port.ToString()+ "'");

		PeerList.Add(Self.GetTree().GetNetworkUniqueId());
		Game.SpawnPlayer(Self.GetTree().GetNetworkUniqueId(), true);
	}


	public static void ConnectTo(string InIp)
	{
		Ip = InIp;

		NetworkedMultiplayerENet Peer = new NetworkedMultiplayerENet();
		Peer.CreateClient(Ip, Port);
		Self.GetTree().SetNetworkPeer(Peer);
		Self.GetTree().SetMeta("network_peer", Peer);
	}


	[Remote]
	public void ReceiveMessage(MESSAGE RecievedMessage, object[] Args)
	{
		int Sender = Self.GetTree().GetRpcSenderId();
		if(Sender == 0)
		{
			//When Sender it 0 that means that ReciveMessage was called locally
			Sender = Self.GetTree().GetNetworkUniqueId();
		}

		if(Self.GetTree().IsNetworkServer())
		{ //Runs on server, 100% trusted
			switch(RecievedMessage)
			{
				case(MESSAGE.PLAYER_REQUEST_POS):{
					Perform.RemotePlayerMove(Events.INVOKER.SERVER, Sender, (Vector3)Args[0]);
					foreach(int Peer in PeerList)
					{
						if(Peer != Sender && Peer != Self.GetTree().GetNetworkUniqueId()) //Don't notify original client or server, both already know
						{
							Message.ServerUpdatePlayerPos(Peer, Sender, (Vector3)Args[0]);
						}
					}
					return;
				}

				case(MESSAGE.PLAYER_REQUEST_ROT):{
					Perform.RemotePlayerRotate(Events.INVOKER.SERVER, Sender, (float)Args[0]);
					foreach(int Peer in PeerList)
					{
						if(Peer != Sender && Peer != Self.GetTree().GetNetworkUniqueId()) //Don't notify original client or server, both already know
						{
							Message.ServerUpdatePlayerRot(Peer, Sender, (float)Args[0]);
						}
					}
					return;
				}

				case(MESSAGE.PLACE_REQUEST):{
					Perform.Place(Events.INVOKER.SERVER, (int)Args[0], (Items.TYPE)Args[1], (Vector3)Args[2], (Vector3)Args[3]);
					return;
				}
			}
		}

		switch(RecievedMessage)
		{
			case(MESSAGE.PLAYER_UPDATE_POS):{
				Perform.RemotePlayerMove(Events.INVOKER.CLIENT, (int)Args[0], (Vector3)Args[1]);
				return;
			}

			case(MESSAGE.PLAYER_UPDATE_ROT):{
				Perform.RemotePlayerRotate(Events.INVOKER.CLIENT, (int)Args[0], (float)Args[1]);
				return;
			}

			case(MESSAGE.PEERLIST_UPDATE):{
				PeerList.Clear();
				foreach(int Peer in (int[])(Args[0]))
				{
					PeerList.Add(Peer);
				}
				return;
			}

			default:{
				Console.Log("Invalid message '" + RecievedMessage.ToString() + "'");
				return;
			}
		}
	}


	public static void SendMessage(int Id, MESSAGE Message, object[] Args)
	{
		if(Self.GetTree().GetNetworkUniqueId() == Id)
		{
			Self.ReceiveMessage(Message, Args);
			return;
		}

		Self.RpcId(Id, "ReceiveMessage", new object[] {Message, Args});
	}


	public static void SendUnreliableMessage(int Id, MESSAGE Message, object[] Args)
	{
		if(Self.GetTree().GetNetworkUniqueId() == Id)
		{
			Self.ReceiveMessage(Message, Args);
			return;
		}

		Self.RpcUnreliableId(Id, "ReceiveMessage", new object[] {Message, Args});
	}
}
