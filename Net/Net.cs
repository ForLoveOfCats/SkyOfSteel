using Godot;
using System;
using System.Linq;
using System.Collections.Generic;


public class Net : Node
{
	private const double VersionDisconnectDelay = 10; /*How many seconds the server will wait for a client to identify
	                                                    their version before disconnecting from a client which refuses
	                                                    to identify their version*/

	public static int ServerId = 1;

	private static int Port = 27015;
	private static string Ip;

	public static List<int> PeerList = new List<int>();
	public static Dictionary<int, double> WaitingForVersion = new Dictionary<int, double>();

	public static Dictionary<int, string> Nicknames = new Dictionary<int, string>();

	public static Net Self;
	Net()
	{
		if(Engine.EditorHint) {return;}

		Self = this;
	}


	public override void _Ready()
	{
		GetTree().Connect("network_peer_connected", this, "_PlayerConnected");
		GetTree().Connect("network_peer_disconnected", this, "_PlayerDisconnected");
		GetTree().Connect("server_disconnected", this, "_ServerDisconnected");
	}


	public static void SteelRpc(Node Instance, string Method, params object[] Args) //Doesn't rpc clients which are not ready
	{
		foreach(int Id in PeerList)
		{
			if(Id == Self.GetTree().GetNetworkUniqueId())
			{
				continue;
			}
			Instance.RpcId(Id, Method, Args);
		}
	}


	public static void SteelRpcUnreliable(Node Instance, string Method, params object[] Args) //Doesn't rpc clients which are not ready
	{
		foreach(int Id in PeerList)
		{
			if(Id == Self.GetTree().GetNetworkUniqueId())
			{
				continue;
			}
			Instance.RpcUnreliableId(Id, Method, Args);
		}
	}


	public void _PlayerConnected(int Id)
	{
		if(Id == 1) //Running on client and connected to server
		{
			RpcId(ServerId, nameof(ProvideVersion), Game.Version);
		}
		else //Connected to a client OR server
		{
			Console.Log("Player '" + Id.ToString() + "' connected");
		}

		if(GetTree().IsNetworkServer())
		{
			//If we are the server
			WaitingForVersion.Add(Id, 0d); //then add new client to WaitingForVersion
		}
	}


	[Remote]
	public void ProvideVersion(string Version) //Run on server
	{
		if(!GetTree().IsNetworkServer())
		{
			return; //Make sure we really are the server
		}

		//The client sent its version and we are running on the server
		if(Version != Game.Version) //Version mismatch
		{
			((NetworkedMultiplayerENet)GetTree().GetNetworkPeer()).DisconnectPeer(GetTree().GetRpcSenderId());
			WaitingForVersion.Remove(GetTree().GetRpcSenderId());
			return;
		}

		WaitingForVersion.Remove(GetTree().GetRpcSenderId());

		Building.RemoteLoadedChunks.Add(GetTree().GetRpcSenderId(), new List<Tuple<int,int>>());

		SetupNewPeer(GetTree().GetRpcSenderId());
		RpcId(GetTree().GetRpcSenderId(), nameof(NotifySuccessConnect));
		SteelRpc(this, nameof(SetupNewPeer), GetTree().GetRpcSenderId());
		foreach(int Id in PeerList)
		{
			if(Id == GetTree().GetRpcSenderId())
			{
				continue;
			}
			RpcId(GetTree().GetRpcSenderId(), nameof(SetupNewPeer), Id);
		}

		if(Scripting.ClientGmScript != null)
		{
			Scripting.Self.RpcId(GetTree().GetRpcSenderId(), nameof(Scripting.NetLoadClientScript), new object[] {Scripting.ClientGmScript});
		}

		RpcId(GetTree().GetRpcSenderId(), nameof(ReadyToRequestWorld), new object[] {});
	}


	[Remote]
	public void NotifySuccessConnect() //Run on client
	{
		Console.Log("Connected to server at '" + Ip.ToString() + "'");
		Game.StartWorld();
		PeerList.Add(Self.GetTree().GetNetworkUniqueId());
		Game.SpawnPlayer(Self.GetTree().GetNetworkUniqueId(), true);

		RpcId(ServerId, nameof(RecieveNick), GetTree().GetNetworkUniqueId(),  Game.Nickname);
	}


	[Remote]
	public void SetupNewPeer(int Id) //Run on all clients except for the new client
	{
		if(Id == GetTree().GetNetworkUniqueId())
		{
			return; //Make sure we are not the new peer
		}

		Game.SpawnPlayer(Id, false);
		PeerList.Add(Id);
	}


	[Remote]
	public void RecieveNick(int Id, string NickArg)
	{
		Nicknames[Id] = NickArg;

		if(Id != GetTree().GetNetworkUniqueId())
		{
			Game.PossessedPlayer.HUDInstance.AddNickLabel(Id, NickArg);
		}

		if(GetTree().IsNetworkServer())
		{
			foreach(KeyValuePair<int, string> Entry in Nicknames)
			{
				SteelRpc(this, nameof(RecieveNick), Entry.Key, Entry.Value);
			}
		}
	}


	public void _PlayerDisconnected(int Id)
	{
		Console.Log("Player '" + Id.ToString() + "' disconnected");

		if(PeerList.Contains(Id)) //May be disconnecting from a client which did not fully connect
		{
			Self.GetTree().GetRoot().GetNode("RuntimeRoot/SkyScene/" + Id.ToString()).QueueFree();
			PeerList.Remove(Id);
		}
		if(Nicknames.ContainsKey(Id))
		{
			Nicknames.Remove(Id);
			Game.PossessedPlayer.HUDInstance.RemoveNickLabel(Id);
		}
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
				Console.ThrowPrint("Cannot host when already hosting");
			}
			else
			{
				Console.ThrowPrint("Cannot host when connected to a server");
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
		Nicknames[ServerId] = Game.Nickname;
		Game.SpawnPlayer(Self.GetTree().GetNetworkUniqueId(), true);
	}


	public static void ConnectTo(string InIp)
	{
		if(Self.GetTree().NetworkPeer != null)
		{
			if(Self.GetTree().IsNetworkServer())
			{
				Console.ThrowPrint("Cannot connect when hosting");
			}
			else
			{
				Console.ThrowPrint("Cannot connect when already connected to a server");
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

		NetworkedMultiplayerENet EN = Self.GetTree().GetNetworkPeer() as NetworkedMultiplayerENet;
		if(EN != null)
		{
			EN.CloseConnection();
		}

		Self.GetTree().SetNetworkPeer(null);
		PeerList.Clear();
		Game.PlayerList.Clear();
		Nicknames.Clear();
		Game.PlayerList.Clear();

		Menu.BuildIntro();
	}


	[Remote]
	public void ReadyToRequestWorld() //Called by server on client when client can request world chunks
	{
		Building.Self.RpcId(ServerId, nameof(Building.InitialNetWorldLoad), Self.GetTree().GetNetworkUniqueId(), Game.PossessedPlayer.Translation, Game.ChunkRenderDistance);
	}


	public static void UnloadAndRequestChunks()
	{
		if(!Game.WorldOpen)
		{
			//World is not setup yet
			//Prevents NullReferenceException
			return;
		}

		List<Tuple<int,int>> ToRemove = new List<Tuple<int,int>>();
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
				if(!Self.GetTree().IsNetworkServer())
				{
					ToRemove.Add(Chunk.Key);
				}
			}
		}
		foreach(Tuple<int,int> Chunk in ToRemove)
		{
			Building.Chunks.Remove(Chunk);
		}

		if(!Self.GetTree().IsNetworkServer())
		{
			Building.Self.RequestChunks(Self.GetTree().GetNetworkUniqueId(), Game.PossessedPlayer.Translation, Game.ChunkRenderDistance);
		}
	}


	public override void _Process(float Delta)
	{
		foreach(int Id in WaitingForVersion.Keys.ToArray())
		{
			WaitingForVersion[Id] += Delta;
			if(WaitingForVersion[Id] >= VersionDisconnectDelay)
			{
				Console.ThrowLog($"Player '{Id}' did not provide their client version and was kicked");
				((NetworkedMultiplayerENet)GetTree().GetNetworkPeer()).DisconnectPeer(Id); //Disconnect clients which didn't send their version in time
				WaitingForVersion.Remove(Id);
			}
		}
	}
}
