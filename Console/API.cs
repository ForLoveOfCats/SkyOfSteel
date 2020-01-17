using Godot;
using System;
using System.Net;
using System.Collections.Generic;


public static class API
{
	public static bool Host()
	{
		if(Game.Nickname == Game.DefaultNickname)
		{
			Console.ThrowPrint("Please set a multiplayer nickname before hosting");
			return false;
		}
		Net.Host();
		return true;
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


			IPAddress Address; //Unused, just to check if valid ip
			if(!IPAddress.TryParse(Ip, out Address)) //Requires an `out` argument
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


	public static bool Fly(bool NewFly)
	{
		Game.PossessedPlayer.SetFly(NewFly);
		return NewFly;
	}


	public static bool ToggleFly()
	{
		Game.PossessedPlayer.SetFly(!Game.PossessedPlayer.FlyMode);
		return Game.PossessedPlayer.FlyMode;
	}


	public static bool Give(Items.ID Type) //TODO: Allow as client and giving items to other players
	{
		if(!Net.Work.IsNetworkServer())
		{
			Console.ThrowPrint("Cannot give item as client");
			return false;
		}

		Game.PossessedPlayer.ItemGive(new Items.Instance(Type));
		return true;
	}


	public static void HudHide()
	{
		Game.PossessedPlayer.HUDInstance.Hide();
	}


	public static void HudShow()
	{
		Game.PossessedPlayer.HUDInstance.Show();
	}


	public static bool ChunkRenderDistance(int Distance)
	{
		Game.ChunkRenderDistance = Distance;
		World.UnloadAndRequestChunks();
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


	public static void Quit()
	{
		Game.Quit();
	}
}
