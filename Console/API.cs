using Godot;
using System;
using System.Net;
using System.Collections.Generic;


public static class API
{
	private static bool ArgCountMismatch(string[] Args, int Expected)
	{
		bool Mismatch = Args.Length != Expected;

		if(Mismatch)
			Console.ThrowPrint(
				$"Expected {Expected} arguments but recieved {Args.Length} arguments"
			);

		return Mismatch;
	}


	public static void Help(string[] Args)
	{
		if(Args.Length == 0)
		{
			Console.Print("All commands:");
			foreach(KeyValuePair<string, Backend.CommandInfo> Command in Backend.Commands)
				Console.Print($"  {Command.Key}: {Command.Value.HelpMessage}");
		}
		else
		{
			if(ArgCountMismatch(Args, 1))
				return;

			if(Backend.Commands.TryGetValue(Args[0], out var Command))
				Console.Print($"{Args[0]}: {Command.HelpMessage}");
			else
			{
				Console.ThrowPrint(
					$"No command '{Args[0]}', try running 'help' to view  a list of all commands"
				);
				return;
			}
		}
	}


	public static void Host(string[] Args)
	{
		if(ArgCountMismatch(Args, 2))
			return;

		if(Game.Nickname == Game.DefaultNickname)
		{
			Console.ThrowPrint("Please set a multiplayer nickname before hosting");
			return;
		}

		string Mode = Args[0];
		string Name = Args[1];
		string Path = $"{OS.GetUserDataDir()}/Saves/{Name}";

		if(Mode == "new")
		{
			if(System.IO.Directory.Exists(Path))
			{
				Console.ThrowPrint($"Savefile '{Name}' already exists");
				return;
			}

			System.IO.Directory.CreateDirectory(Path);
		}
		else if(Mode == "existing")
		{
			if(!System.IO.Directory.Exists(Path))
			{
				Console.ThrowPrint($"No savefile named '{Name}'");
				return;
			}
		}
		else
		{
			Console.ThrowPrint($"Expected 'new' or 'existing' but found '{Mode}'");
			return;
		}

		Net.Host();
		World.Load(Name);
	}


	public static bool Connect(string Ip)
	{
		if(Net.Work.NetworkPeer != null)
		{
			if(Net.Work.IsNetworkServer())
			{
				Console.ThrowPrint("Cannot connect when hosting");
			}
			else
			{
				Console.ThrowPrint("Cannot connect when already connected to a server");
			}
			return false;
		}

		if(Game.Nickname == Game.DefaultNickname)
		{
			Console.ThrowPrint("Please set a multiplayer nickname before connecting");
			return false;
		}
		else
		{
			if(Ip == "" || Ip == "localhost")
				Ip = "127.0.0.1";

			if(!IPAddress.TryParse(Ip, out IPAddress Address))
			{
				Console.ThrowPrint("Please provide a valid IP address");
				return false;
			}

			Net.ConnectTo(Ip);
			return true;
		}
	}


	public static bool Disconnect()
	{
		if(!World.IsOpen)
		{
			Console.ThrowPrint("Neither connected nor hosting");
			return false;
		}

		Net.Disconnect();
		return true;
	}


	public static string Nickname(string NewNick)
	{
		if(World.IsOpen)
		{
			Console.ThrowPrint("Cannot set nickname while hosting or connected");
			return Game.Nickname;
		}

		if(NewNick == "")
		{
			Console.ThrowPrint("Please specify a nickname");
			return Game.Nickname;
		}

		Game.Nickname = NewNick;
		return NewNick;
	}


	public static bool Bind(string InputString, string FunctionName)
	{
		return Bindings.Bind(InputString, FunctionName);
	}


	public static bool Unbind(string InputString)
	{
		Bindings.UnBind(InputString);
		return true;
	}


	public static void Fly(bool NewFly)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) => Plr.SetFly(NewFly)
		);
	}


	public static void ToggleFly()
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) => Plr.SetFly(!Plr.FlyMode)
		);
	}


	public static void Give(Items.ID Type) //TODO: Allow as client and giving items to other players
	{
		if(!Net.Work.IsNetworkServer())
		{
			Console.ThrowPrint("Cannot give item as client");
			return;
		}

		Game.PossessedPlayer.MatchSome(
			(Plr) => Plr.ItemGive(new Items.Instance(Type))
		);
	}


	public static void HudHide()
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) => Plr.HUDInstance.Hide()
		);
	}


	public static void HudShow()
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) => Plr.HUDInstance.Show()
		);
	}


	public static bool ChunkRenderDistance(int Distance)
	{
		Game.ChunkRenderDistance = Distance;
		Game.PossessedPlayer.MatchSome(
			(Plr) => World.UnloadAndRequestChunks(Plr.Translation, Game.ChunkRenderDistance)
		);
		return true;
	}


	public static void FpsMax(int TargetFps)
	{
		if(TargetFps <= 1)
		{
			Console.ThrowPrint("Please provide a valid fps value which is greater than 1");
			return;
		}

		Engine.TargetFps = Convert.ToInt32(TargetFps);
	}


	public static bool Save(string Name)
	{
		if(World.IsOpen && Net.Work.NetworkPeer != null && Net.Work.IsNetworkServer())
		{
			if(Name == "")
			{
				Console.ThrowPrint("Please provide a name to save under");
				return false;
			}

			World.Save(Name);
			Console.Print($"Saved world to save '{Name}' successfully");
			return true;
		}
		else
		{
			Console.ThrowPrint("Cannot save world when not hosting");
			return false;
		}
	}


	public static bool Load(string Name)
	{
		if(World.IsOpen && Net.Work.NetworkPeer != null && Net.Work.IsNetworkServer())
		{
			if(Name == "")
			{
				Console.ThrowPrint("Please provide the name of a save to load");
				return false;
			}

			if(World.Load(Name))
			{
				Console.Print($"Loaded save '{Name}' successfully");
				return true;
			}
			Console.Print($"Failed to load save '{Name}");
			return false;
		}
		else
		{
			Console.ThrowPrint("Cannot load savegame when not hosting");
			return false;
		}
	}


	public static bool ReloadSave()
	{
		if(World.IsOpen && Net.Work.NetworkPeer != null && Net.Work.IsNetworkServer())
		{
			World.Load(World.SaveName);
			return true;
		}
		else
		{
			Console.ThrowPrint("Cannot reload savegame when not hosting");
			return false;
		}
	}
}
