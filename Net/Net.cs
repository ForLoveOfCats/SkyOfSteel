using Godot;
using System;
using System.Collections.Generic;


public class Net : Node
{
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
				// Message.NetPlaceSync(Branch.Type, Branch.Translation, Branch.RotationDegrees, Branch.OwnerId, Branch.GetName());
				Building.Self.RpcId(Id, "PlaceWithName", new object[] {Branch.Type, Branch.Translation, Branch.RotationDegrees, Branch.OwnerId, Branch.GetName()});
			}
			
			//Send client gm script
			if(Scripting.ClientGmScript != null)
			{
				Scripting.Self.Rpc("NetLoadClientScript", new object[] {Scripting.ClientGmScript});
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
}
