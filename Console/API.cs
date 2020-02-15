using Godot;
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
			{
				foreach(string Message in Command.Value.HelpMessages)
					Console.Print($"  {Message}");
			}
		}
		else
		{
			if(ArgCountMismatch(Args, 1))
				return;

			string CommandName = Args[0];

			if(Backend.Commands.TryGetValue(CommandName, out var Command))
			{
				foreach(string Message in Command.HelpMessages)
					Console.Print($"  {Message}");
			}
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


	public static void Give(string[] Args)
	{
		if(!Net.Work.IsNetworkServer())
		{
			Console.ThrowPrint("Must be the host player to give items");
			return;
		}

		if(ArgCountMismatch(Args, 3))
			return;

		string PlayerName = Args[0];
		string TypeString = Args[1];
		string CountString = Args[2];

		Player TargetPlayer = null;
		foreach(KeyValuePair<int, string> CurrentNick in Net.Nicknames)
		{
			if(CurrentNick.Value == PlayerName)
				TargetPlayer = Net.Players[CurrentNick.Key].Plr.ValueOr((Player)null);
		}
		if(TargetPlayer is null)
		{
			Console.ThrowPrint($"No player named '{PlayerName}'");
			return;
		}

		Items.ID Id = Items.ID.ERROR;
		foreach(Items.ID CurrentId in System.Enum.GetValues(typeof(Items.ID)))
		{
			if(CurrentId.ToString() == TypeString)
			{
				Id = CurrentId;
				break;
			}
		}
		if(Id == Items.ID.ERROR)
		{
			Console.ThrowPrint($"No item type by the name '{TypeString}'");
			return;
		}

		if(int.TryParse(CountString, out int GiveCount) && GiveCount > 0)
			TargetPlayer.ItemGive(new Items.Instance(Id) { Count = GiveCount });
		else
		{
			Console.ThrowPrint($"Invalid item count '{CountString}'");
			return;
		}
	}


	public static void ChunkRenderDistance(string[] Args)
	{
		if(ArgCountMismatch(Args, 1))
			return;

		string DistanceString = Args[0];

		if(int.TryParse(DistanceString, out int Distance) && Distance > 1)
		{
			Game.ChunkRenderDistance = Distance;
			Game.PossessedPlayer.MatchSome(
				(Plr) => World.UnloadAndRequestChunks(Plr.Translation, Game.ChunkRenderDistance)
			);
		}
		else
			Console.ThrowPrint($"Invalid chunk render distance {DistanceString}");
	}


	public static void FpsMax(string[] Args)
	{
		if(Args.Length == 0)
		{
			Console.Print($"Current max fps: {Engine.TargetFps}");
			return;
		}

		if(ArgCountMismatch(Args, 1))
			return;

		string TargetString = Args[0];

		if(int.TryParse(TargetString, out int Target) && Target >= 10)
			Engine.TargetFps = Target;
		else
			Console.ThrowPrint($"Invalid max fps {TargetString}");
	}
}
