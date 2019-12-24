using Godot;
using System;
using System.Linq;
using System.Collections.Generic;


public class Net : Node
{
	private const int MaxWaitForServerDelay = 10;
	private const float VersionDisconnectDelay = 10; /*How many seconds the server will wait for a client to identify
	                                                    their version before disconnecting from a client which refuses
	                                                    to identify their version*/

	public static int ServerId = 1;

	private static int Port = 27015;
	public static string Ip { get; private set; }

	public static List<int> PeerList = new List<int>();
	public static Dictionary<int, Player> Players = new Dictionary<int, Player>();
	public static Dictionary<int, float> WaitingForVersion = new Dictionary<int, float>();
	public static bool IsWaitingForServer { get; private set; } = false;
	public static float WaitingForServerTimer { get; private set; } = MaxWaitForServerDelay;

	public static Dictionary<int, string> Nicknames = new Dictionary<int, string>();

	public static MultiplayerAPI Work; //Get it? Net.Work

	public static Net Self;
	Net()
	{
		if(Engine.EditorHint) {return;}

		Self = this;
	}


	public override void _Ready()
	{
		Work = Multiplayer; //This means that anywhere we can Net.Work.Whatever instead of Game.Self.GetTree().Whatever

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
			IsWaitingForServer = false;
			WaitingForServerTimer = MaxWaitForServerDelay;

			RpcId(ServerId, nameof(ProvideVersion), Game.Version);
		}
		else //Connected to a client OR server
		{
			Console.Log($"Player '{Id}' connected");
		}

		if(GetTree().IsNetworkServer())
		{
			//If we are the server
			WaitingForVersion.Add(Id, 0); //then add new client to WaitingForVersion
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
			((NetworkedMultiplayerENet)GetTree().NetworkPeer).DisconnectPeer(GetTree().GetRpcSenderId());
			WaitingForVersion.Remove(GetTree().GetRpcSenderId());
			return;
		}

		WaitingForVersion.Remove(GetTree().GetRpcSenderId());

		World.RemoteLoadedChunks.Add(GetTree().GetRpcSenderId(), new List<Tuple<int,int>>());

		RpcId(GetTree().GetRpcSenderId(), nameof(NotifySuccessConnect));
		SetupNewPeer(GetTree().GetRpcSenderId());
		SteelRpc(this, nameof(SetupNewPeer), GetTree().GetRpcSenderId());
		foreach(int Id in PeerList)
		{
			if(Id == GetTree().GetRpcSenderId())
			{
				continue;
			}
			RpcId(GetTree().GetRpcSenderId(), nameof(SetupNewPeer), Id);
		}

		RpcId(GetTree().GetRpcSenderId(), nameof(ReadyToRequestWorld), new object[] {});
	}


	[Remote]
	public void NotifySuccessConnect() //Run on client
	{
		Console.Log($"Connected to server at '{Ip}'");
		World.Start();
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
		World.ChunkLoadDistances[Id] = 0;
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
		Console.Log($"Player '{Id}' disconnected");

		if(PeerList.Contains(Id)) //May be disconnecting from a client which did not fully connect
		{
			Self.GetTree().Root.GetNode($"RuntimeRoot/SkyScene/{Id}").QueueFree();
			PeerList.Remove(Id);
		}
		Players.Remove(Id);

		if(Nicknames.ContainsKey(Id))
		{
			Nicknames.Remove(Id);
			Game.PossessedPlayer.HUDInstance.RemoveNickLabel(Id);
		}

		World.ChunkLoadDistances.Remove(Id);
		World.RemoteLoadedChunks.Remove(Id);
	}


	public void _ServerDisconnected()
	{
		Console.Log($"Lost connection to server at '{Ip}'");
		Disconnect();
	}


	public static void Host()
	{
		if(Self.GetTree().NetworkPeer != null)
		{
			Console.ThrowPrint(Self.GetTree().IsNetworkServer()
				? "Cannot host when already hosting"
				: "Cannot host when connected to a server");
			return;
		}

		PeerList.Clear();
		World.Start();

		var Peer = new NetworkedMultiplayerENet();
		Peer.CreateServer(Port, Game.MaxPlayers);
		Self.GetTree().NetworkPeer = Peer;
		Self.GetTree().SetMeta("network_peer", Peer);

		Console.Log($"Started hosting on port '{Port}'");

		PeerList.Add(Self.GetTree().GetNetworkUniqueId());
		Nicknames[ServerId] = Game.Nickname;
		Game.SpawnPlayer(Self.GetTree().GetNetworkUniqueId(), true);

		World.DefaultPlatforms();
	}


	[Signal]
	public delegate void ConnectToFailed(string Ip);


	public static void ConnectTo(string InIp)
	{
		//Set static string Ip
		Ip = InIp;

		NetworkedMultiplayerENet Peer = new NetworkedMultiplayerENet();
		Peer.CreateClient(Ip, Port);
		Self.GetTree().NetworkPeer = Peer;

		IsWaitingForServer = true;
	}


	public static void Disconnect(bool BuildMenu = true)
	{
		World.Close();

		NetworkedMultiplayerENet EN = Self.GetTree().NetworkPeer as NetworkedMultiplayerENet;
		if(EN != null)
			EN.CloseConnection();

		Self.GetTree().NetworkPeer = null;
		PeerList.Clear();
		Net.Players.Clear();
		Nicknames.Clear();
		Net.Players.Clear();

		IsWaitingForServer = false;
		WaitingForServerTimer = MaxWaitForServerDelay;

		if(BuildMenu)
		{
			Menu.BuildMain();
		}
	}


	[Remote]
	public void ReadyToRequestWorld() //Called by server on client when client can request world chunks
	{
		World.Self.RpcId(ServerId, nameof(World.InitialNetWorldLoad), Self.GetTree().GetNetworkUniqueId(), Game.PossessedPlayer.Translation, Game.ChunkRenderDistance);
	}


	public static void UnloadAndRequestChunks() //TODO: Why is this in Net?
	{
		if(!World.IsOpen)
		{
			//World is not setup yet
			//Prevents NullReferenceException
			return;
		}

		foreach(KeyValuePair<System.Tuple<int, int>, ChunkClass> Chunk in World.Chunks.ToArray())
		{
			Vector3 ChunkPos = new Vector3(Chunk.Key.Item1, 0, Chunk.Key.Item2);
			if(ChunkPos.DistanceTo(new Vector3(Game.PossessedPlayer.Translation.x,0,Game.PossessedPlayer.Translation.z)) <= Game.ChunkRenderDistance*(World.PlatformSize*9))
			{
				if(Self.GetTree().IsNetworkServer())
				{
					foreach(Tile CurrentTile in Chunk.Value.Tiles)
					{
						CurrentTile.Show();
					}

					foreach(MobClass Mob in Chunk.Value.Mobs)
					{
						Mob.Show();
					}

					foreach(DroppedItem Item in Chunk.Value.Items)
					{
						Item.Show();
					}
				}
			}
			else
			{
				List<Tile> TilesBeingRemoved = new List<Tile>();
				foreach(Tile CurrentTile in Chunk.Value.Tiles)
				{
					if(Self.GetTree().IsNetworkServer())
					{
						CurrentTile.Hide();
					}
					else
					{
						TilesBeingRemoved.Add(CurrentTile);
					}
				}
				foreach(Tile CurrentTile in TilesBeingRemoved)
				{
						CurrentTile.Remove(Force:true);
				}

				List<MobClass> MobsBeingRemoved = new List<MobClass>();
				foreach(MobClass Mob in Chunk.Value.Mobs)
				{
					if(Self.GetTree().IsNetworkServer())
					{
						Mob.Hide();
					}
					else
					{
						MobsBeingRemoved.Add(Mob);
					}
				}
				foreach(MobClass Mob in MobsBeingRemoved)
				{
					Mob.QueueFree();
				}

				List<DroppedItem> ItemsBeingRemoved = new List<DroppedItem>();
				foreach(DroppedItem Item in Chunk.Value.Items)
				{
					if(Self.GetTree().IsNetworkServer())
					{
						Item.Hide();
					}
					else
					{
						ItemsBeingRemoved.Add(Item);
					}
				}
				foreach(DroppedItem Item in ItemsBeingRemoved)
				{
					Item.Remove();
				}
			}
		}


		if(!Self.GetTree().IsNetworkServer())
		{
			World.Self.RequestChunks(Self.GetTree().GetNetworkUniqueId(), Game.PossessedPlayer.Translation, Game.ChunkRenderDistance);
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
				((NetworkedMultiplayerENet)GetTree().NetworkPeer).DisconnectPeer(Id); //Disconnect clients which didn't send their version in time
				WaitingForVersion.Remove(Id);
			}
		}

		if(IsWaitingForServer)
		{
			WaitingForServerTimer -= Delta;
			if(WaitingForServerTimer <= 0)
			{
				Self.EmitSignal(nameof(ConnectToFailed), Ip);
				Console.ThrowLog($"Failed to connect to '{Ip}'");
				Disconnect(BuildMenu:false);
			}
		}
	}
}
