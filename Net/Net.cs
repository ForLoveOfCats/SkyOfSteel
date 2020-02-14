using Godot;
using Optional;
using System;
using System.Linq;
using System.Collections.Generic;


public class Net : Node
{
	public class PlayerData
	{
		public Option<Player> Plr;
		public int Team = 1;

		public PlayerData()
		{
			Plr = Player.None();
		}
	}


	private const int MaxWaitForServerDelay = 10;
	private const float VersionDisconnectDelay = 10; /*How many seconds the server will wait for a client to identify
	                                                    their version before disconnecting from a client which refuses
	                                                    to identify their version*/

	public static int ServerId = 1;

	public static int Port = 27015;
	public static string Ip { get; private set; }

	public static Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();
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

		GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
		GetTree().Connect("server_disconnected", this, nameof(ServerDisconnected), flags:(uint)ConnectFlags.Deferred);
	}


	public static void SteelRpc(Node Instance, string Method, params object[] Args) //Doesn't rpc clients which are not ready
	{
		foreach(int Id in Players.Keys)
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
		foreach(int Id in Players.Keys)
		{
			if(Id == Self.GetTree().GetNetworkUniqueId())
			{
				continue;
			}
			Instance.RpcUnreliableId(Id, Method, Args);
		}
	}


	public void PlayerConnected(int Id)
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
		World.RemoteLoadingChunks.Add(GetTree().GetRpcSenderId(), new List<Tuple<int,int>>());

		RpcId(GetTree().GetRpcSenderId(), nameof(NotifySuccessConnect));
		SetupNewPeer(GetTree().GetRpcSenderId());
		SteelRpc(this, nameof(SetupNewPeer), GetTree().GetRpcSenderId());
		foreach(int Id in Players.Keys)
		{
			if(Id == GetTree().GetRpcSenderId())
			{
				continue;
			}
			RpcId(GetTree().GetRpcSenderId(), nameof(SetupNewPeer), Id);
		}

		RpcId(GetTree().GetRpcSenderId(), nameof(ReadyToRequestWorld));
	}


	[Remote]
	public void NotifySuccessConnect() //Run on client
	{
		Console.Log($"Connected to server at '{Ip}'");
		World.Start();
		Players.Add(Self.GetTree().GetNetworkUniqueId(), new PlayerData());
		Game.SpawnPlayer(Self.GetTree().GetNetworkUniqueId(), true);

		RpcId(ServerId, nameof(ReceiveNick), GetTree().GetNetworkUniqueId(),  Game.Nickname);
	}


	[Remote]
	public void SetupNewPeer(int Id) //Run on all clients except for the new client
	{
		if(Id == GetTree().GetNetworkUniqueId())
		{
			return; //Make sure we are not the new peer
		}

		Players.Add(Id, new PlayerData());
		Game.SpawnPlayer(Id, false);
		World.ChunkLoadDistances[Id] = 0;
	}


	[Remote]
	public void ReceiveNick(int Id, string NickArg)
	{
		Nicknames[Id] = NickArg;

		if(Id != GetTree().GetNetworkUniqueId())
		{
			Game.PossessedPlayer.MatchSome(
				(Plr) => Plr.HUDInstance.AddNickLabel(Id, NickArg)
			);
		}

		if(GetTree().IsNetworkServer())
		{
			foreach(KeyValuePair<int, string> Entry in Nicknames)
			{
				SteelRpc(this, nameof(ReceiveNick), Entry.Key, Entry.Value);
			}
		}
	}


	public void PlayerDisconnected(int Id)
	{
		Console.Log($"Player '{Id}' disconnected");

		if(Players.ContainsKey(Id)) //May be disconnecting from a client which did not fully connect
		{
			Players[Id].Plr.MatchSome(
				(Plr) => Plr.QueueFree()
			);
			Players.Remove(Id);
		}

		if(Nicknames.ContainsKey(Id))
		{
			Nicknames.Remove(Id);
			Game.PossessedPlayer.MatchSome(
				(Plr) => Plr.HUDInstance.RemoveNickLabel(Id)
			);
		}

		World.ChunkLoadDistances.Remove(Id);
		World.RemoteLoadedChunks.Remove(Id);
	}


	public void ServerDisconnected()
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

		Players.Clear();
		World.Start();

		var Peer = new NetworkedMultiplayerENet();
		Peer.CreateServer(Port, Game.MaxPlayers);
		Self.GetTree().NetworkPeer = Peer;

		Console.Log($"Started hosting on port '{Port}'");

		Players.Add(Self.GetTree().GetNetworkUniqueId(), new PlayerData());
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

		if(Self.GetTree().NetworkPeer is NetworkedMultiplayerENet En)
			En.CloseConnection();

		Self.GetTree().NetworkPeer = null;
		Nicknames.Clear();
		Players.Clear();

		IsWaitingForServer = false;
		WaitingForServerTimer = MaxWaitForServerDelay;

		if(BuildMenu)
			Menu.BuildMain();
	}


	[Remote]
	public void ReadyToRequestWorld() //Called by server on client when client can request world chunks
	{
		World.Self.RpcId(ServerId, nameof(World.InitialNetWorldLoad), Self.GetTree().GetNetworkUniqueId(), new Vector3(), Game.ChunkRenderDistance);
	}


	[Remote]
	public void RequestTeamChange(int NewTeam)
	{
		if(!Net.Work.IsNetworkServer())
		{
			RpcId(ServerId, nameof(RequestTeamChange), NewTeam);
			return;
		}

		int Id = Net.Work.GetRpcSenderId();
		if(Id == 0)
			Id = ServerId; //We are changing our own team

		NotifyTeamChange(Id, NewTeam);
		SteelRpc(this, nameof(NotifyTeamChange), Id, NewTeam);
	}


	[Remote]
	public void NotifyTeamChange(int Id, int NewTeam)
	{
		Players[Id].Team = NewTeam;
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
