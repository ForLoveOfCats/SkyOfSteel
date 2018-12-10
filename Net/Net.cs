using Godot;
using System;
using System.Collections.Generic;


public class Net : Node
{
	public enum MESSAGE {PLACE_REQUEST, PLACE_SYNC, REMOVE_REQUEST, REMOVE_SYNC};
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
		PeerList.Add(Id);

		if(Self.GetTree().IsNetworkServer())
		{
			//Send world to new client
			foreach(Structure Branch in Game.StructureRoot.GetChildren())
			{
				Message.NetPlaceSync(Branch.Type, Branch.Translation, Branch.RotationDegrees, Branch.OwnerId, Branch.GetName());
			}
		}
	}


	public void _PlayerDisconnected(int Id)
	{
		Console.Log("Player '" + Id.ToString() + "' disconnected");
		Self.GetTree().GetRoot().GetNode("RuntimeRoot/SkyScene/" + Id.ToString()).QueueFree();
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
		Game.StartWorld(AsServer: true);

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
				case(MESSAGE.PLACE_REQUEST):{
					Perform.Place(Events.INVOKER.SERVER, (int)Args[0], (Items.TYPE)Args[1], (Vector3)Args[2], (Vector3)Args[3]);
					return;
				}

				case(MESSAGE.REMOVE_REQUEST):{
					Perform.Remove(Events.INVOKER.SERVER, (string)Args[0]);
					return;
				}
			}
		}

		switch(RecievedMessage)
		{
			case(MESSAGE.PLACE_SYNC):{
				Perform.Place(Events.INVOKER.CLIENT, (int)Args[0], (Items.TYPE)Args[1], (Vector3)Args[2], (Vector3)Args[3], (string)Args[4]);
				return;
			}

			case(MESSAGE.REMOVE_SYNC):{
				Perform.Remove(Events.INVOKER.CLIENT, (string)Args[0]);
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
