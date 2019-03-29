using Godot;
using System;
using System.Collections.Generic;


public static class API
{
	public static bool Host()
	{
		if(Game.Nickname == "")
		{
			Console.ThrowPrint("Please set a multiplayer nickname before hosting");
			return false;
		}
		Net.Host();
		return true;
	}


	public static bool Connect(string Ip)
	{
		if(Game.Nickname == "")
		{
			Console.ThrowPrint("Please set a multiplayer nickname before connecting");
			return false;
		}
		else
		{
			if(Ip == "" || Ip == "localhost")
				Ip = "127.0.0.1";

			Net.ConnectTo(Ip); //TODO move error checking/prints into this Connect function
			return true; //TODO This should only return true when Ip is valid
		}
	}


	public static bool Disconnect()
	{
		if(!Game.WorldOpen)
		{
			Console.ThrowPrint("Neither connected nor hosting");
			return false;
		}

		Net.Disconnect();
		return true;
	}


	public static string Nickname(string NewNick)
	{
		if(Game.WorldOpen)
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
		if(Distance < 2)
		{
			Console.ThrowPrint("Cannot set render distance value lower than two chunks");
			return false;
		}

		Game.ChunkRenderDistance = Distance;
		Net.UnloadAndRequestChunks();
		return true;
	}


	public static void FpsMax(int TargetFps)
	{
		if(TargetFps <= 1)
		{
			Console.ThrowPrint("Please provide a valid fps value which is greater than 1");
			return;
		}

		Engine.SetTargetFps(Convert.ToInt32(TargetFps));
	}


	public static bool Save(string Name)
	{
		if(Game.WorldOpen && Net.Work.GetNetworkPeer() != null && Net.Work.IsNetworkServer())
		{
			if(Name == "")
			{
				Console.ThrowPrint("Please provide a name to save under");
				return false;
			}

			Game.SaveWorld(Name);
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
		if(Game.WorldOpen && Net.Work.GetNetworkPeer() != null && Net.Work.IsNetworkServer())
		{
			if(Name == "")
			{
				Console.ThrowPrint("Please provide the name of a save to load");
				return false;
			}

			if(Game.LoadWorld(Name))
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


	public static bool Gamemode(string Name)
	{
		if(Net.Work.GetNetworkPeer() != null && Net.Work.IsNetworkServer())
		{
			Scripting.LoadGameMode(Name); //TODO move error checking into this Gamemode function
			return true;
		}

		Console.ThrowPrint("Cannot set gamemode when not hosting");
		return false;
	}


	public static bool Reload()
	{
		if(Net.Work.GetNetworkPeer() != null && Net.Work.IsNetworkServer())
		{
			if(Scripting.GamemodeName != "")
			{
				Scripting.LoadGameMode(Scripting.GamemodeName);
				return true;
			}

			Console.ThrowPrint("No gamemode loaded to reload");
			return false;
		}

		Console.ThrowPrint("Must be hosting to reload gamemode");
		return false;
	}
}
