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
				Function = (Args) => API.Help(Args)
			}
		},

		{
			"quit",
			new CommandInfo {
				HelpMessage = "Immediately closes the game",
				Function = (Args) => Game.Quit()
			}
		},


		{
			"host",
			new CommandInfo {
				HelpMessage = $"Starts hosting on port {Net.Port}. Specify 'new' or 'existing' followed by savefile name",
				Function = (Args) => API.Host(Args)
			}
		},

		{
			"give",
			new CommandInfo {
				HelpMessage = $"Gives a <player> an <item> of <count>",
				Function = (Args) => API.Give(Args)
			}
		},

		{
			"chunk_render_distance",
			new CommandInfo {
				HelpMessage = $"Sets tile render distance in chunks",
				Function = (Args) => API.ChunkRenderDistance(Args)
			}
		},
	};


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
