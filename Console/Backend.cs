using System.Collections.Generic;



public static class Backend
{
	public delegate void CommandFunction(string[] Args);

	public struct CommandInfo
	{
		public string HelpMessage;
		public CommandFunction Function;
	}


	public static Dictionary<string, CommandInfo> Commands =
		new Dictionary<string, CommandInfo> {
		{
			"help",
			new CommandInfo {
				HelpMessage = "Lists all commands or displays a the help message for an individual command",
				Function = (Args) =>
				{
					if(Args.Length == 0)
					{
						Console.Print("All commands:");
						foreach(KeyValuePair<string, CommandInfo> Command in Commands)
							Console.Print($"  {Command.Key}: {Command.Value.HelpMessage}");
					}
					else
					{
						if(ArgCountMismatch(Args, 1))
							return;

						if(Commands.TryGetValue(Args[0], out CommandInfo Command))
							Console.Print($"{Args[0]}: {Command.HelpMessage}");
						else
						{
							Console.ThrowPrint($"No command '{Args[0]}', try running 'help' to view  a list of all commands");
						}
					}
				}
			}
		},

		{
			"quit",
			new CommandInfo {
				HelpMessage = "Immediately closes the game",
				Function = (Args) => Game.Quit()
			}
		},
	};


	public static bool ArgCountMismatch(string[] Args, int Expected)
	{
		bool Mismatch = Args.Length != Expected;

		if(Mismatch)
			Console.ThrowPrint($"Expected {Expected} arguments but recieved {Args.Length} arguments");

		return Mismatch;
	}


	public static void RunCommand(string Line)
	{

		string[] Split = Line.Split(null);
		if(Split.Length >= 1)
		{
			string Name = Split[0];

			string[] Args = new string[Split.Length - 1];
			for(int Index = 1; Index < Split.Length; Index += 1)
				Args[Index - 1] = Split[Index];

			foreach(KeyValuePair<string, CommandInfo> Command in Commands)
			{
				if(Command.Key == Name)
				{
					Command.Value.Function(Args);
					return;
				}
			}

			Console.ThrowPrint($"No command '{Name}', try running 'help' to view  a list of all commands");
		}
	}
}
