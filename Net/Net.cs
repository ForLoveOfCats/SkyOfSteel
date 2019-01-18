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

		Building.RemoteLoadedChunks.Add(Id, new List<Tuple<int,int>>());

		if(GetTree().IsNetworkServer())
		{
			if(Scripting.ClientGmScript != null)
			{
				Scripting.Self.RpcId(Id, nameof(Scripting.NetLoadClientScript), new object[] {Scripting.ClientGmScript});
			}

			RpcId(Id, nameof(ReadyToRequestWorld), new object[] {});
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
		Disconnect();
	}


	public static void Host()
	{
		if(Self.GetTree().NetworkPeer != null)
		{
			if(Self.GetTree().IsNetworkServer())
			{
				Console.Print("Error: Cannot host when already hosting");
			}
			else
			{
				Console.Print("Error: Cannot host when connected to a server");
			}
			return;
		}

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
		if(Self.GetTree().NetworkPeer != null)
		{
			if(Self.GetTree().IsNetworkServer())
			{
				Console.Print("Error: Cannot connect when hosting");
			}
			else
			{
				Console.Print("Error: Cannot connect when already connected to a server");
			}
			return;
		}

		//Set static string Ip
		Ip = InIp;

		NetworkedMultiplayerENet Peer = new NetworkedMultiplayerENet();
		Peer.CreateClient(Ip, Port);
		Self.GetTree().SetNetworkPeer(Peer);
	}


	public static void Disconnect()
	{
		Game.CloseWorld();
		Self.GetTree().NetworkPeer.Dispose();
		Self.GetTree().SetNetworkPeer(null);
		PeerList.Clear();
		Game.PlayerList.Clear();
	}


	[Remote]
	public void ReadyToRequestWorld() //Called by server on client when client can request world chunks
	{
		UnloadAndRequestChunks();
	}


	public static void UnloadAndRequestChunks()
	{
		if(Game.StructureRoot == null)
		{
			//World is not setup yet
			//Prevents NullReferenceException
			return;
		}

		foreach(KeyValuePair<System.Tuple<int, int>, List<Structure>> Chunk in Building.Chunks)
		{
			Vector3 ChunkPos = new Vector3(Chunk.Key.Item1, 0, Chunk.Key.Item2);
			if(ChunkPos.DistanceTo(new Vector3(Game.PossessedPlayer.Translation.x,0,Game.PossessedPlayer.Translation.z)) <= Game.ChunkRenderDistance*(Building.PlatformSize*9))
			{
				if(Self.GetTree().IsNetworkServer())
				{
					foreach(Structure CurrentStructure in Chunk.Value)
					{
						CurrentStructure.Show();
					}
				}
			}
			else
			{
				foreach(Structure CurrentStructure in Chunk.Value)
				{
					if(Self.GetTree().IsNetworkServer())
					{
						CurrentStructure.Hide();
					}
					else
					{
						CurrentStructure.QueueFree();
					}
				}
			}
		}

		if(!Self.GetTree().IsNetworkServer())
		{
			Building.Self.RequestChunks(Self.GetTree().GetNetworkUniqueId(), Game.PossessedPlayer.Translation, Game.ChunkRenderDistance);
		}
	}
}
