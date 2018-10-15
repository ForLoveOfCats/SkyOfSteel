using Godot;
using System;
using System.Collections.Generic;


public class Net : Node
{
	public enum MESSAGE {PLAYER_REQUEST_POS, PLAYER_REQUEST_ROT, UPDATE_PLAYER_POS, UPDATE_PLAYER_ROT};
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
		}
		else
		{
			Console.Log("Player '" + Id.ToString() + "' connected");
		}
		Game.SpawnPlayer(Id.ToString(), false);
		PeerList.Add(Id);
	}


	public void _PlayerDisconnected(int Id)
	{
		Console.Log("Player '" + Id.ToString() + "' disconnected");
		Self.GetTree().GetRoot().GetNode("SteelGame/SkyScene/" + Id.ToString()).QueueFree();
		PeerList.Remove(Id);
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
	public void ReceiveMessage(MESSAGE Message, object[] Args)
	{
		int Sender = Self.GetTree().GetRpcSenderId();
		if(Sender == 0)
		{
			Sender = 1;
		}

		if(Self.GetTree().IsNetworkServer())
		{ //Runs on server, 100% trusted
			switch(Message)
			{
				case(MESSAGE.PLAYER_REQUEST_POS):{
					Spatial Player = (Spatial)Self.GetTree().GetRoot().GetNode("SteelGame/SkyScene/" + Sender.ToString());
					Player.Translation = (Vector3)Args[0];
					foreach(int Peer in PeerList)
					{
						if(Peer != Sender && Peer != Self.GetTree().GetNetworkUniqueId()) //Don't notify original client or server, both already know
						{
							SendUnreliableMessage(Peer, MESSAGE.UPDATE_PLAYER_POS, new object[] {Sender, Args[0]});
						}
					}
					return;
				}

				case(MESSAGE.PLAYER_REQUEST_ROT):{
					Spatial Player = (Spatial)Self.GetTree().GetRoot().GetNode("SteelGame/SkyScene/" + Sender.ToString());
					Player.SetRotationDegrees(new Vector3(0, (float)Args[0], 0));
					foreach(int Peer in PeerList)
					{
						if(Peer != Sender && Peer != Self.GetTree().GetNetworkUniqueId()) //Don't notify original client or server, both already know
						{
							SendUnreliableMessage(Peer, MESSAGE.UPDATE_PLAYER_ROT, new object[] {Sender, Args[0]});
						}
					}
					return;
				}
			}
		}

		switch(Message)
		{
			case(MESSAGE.UPDATE_PLAYER_POS):{
				Spatial Player = (Spatial)Self.GetTree().GetRoot().GetNode("SteelGame/SkyScene/" + Args[0].ToString());
				Player.Translation = (Vector3)Args[1];
				return;
			}

			case(MESSAGE.UPDATE_PLAYER_ROT):{
				Spatial Player = (Spatial)Self.GetTree().GetRoot().GetNode("SteelGame/SkyScene/" + Args[0].ToString());
				Player.SetRotationDegrees(new Vector3(0, (float)Args[1], 0));
				return;
			}

			default:{
				Console.Log("Invalid message '" + Message.ToString() + "'");
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
