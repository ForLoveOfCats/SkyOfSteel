using Godot;
// using Jurassic;


public class Startup : Node
{
	//Processes command line arguments and runs autoexec.js
	public override void _Ready()
	{
		//Command line arguments are processed first
		string[] CmdArgs = OS.GetCmdlineArgs();
		foreach(string CurrentArg in CmdArgs)
		{
			Console.Log($"Command line argument '{CurrentArg}'");

			switch(CurrentArg)
			{
				case "-host": {
					Net.Host();
					break;
				}

				case "-connect": {
					Net.ConnectTo("127.0.0.1");
					break;
				}
			}
		}

		if(CmdArgs.Length > 0)
		{
			Console.Log("");
		}


		//autoexec.js is executed afterwards
		/*File Autoexec = new File();
		if(Autoexec.FileExists("user://autoexec.js"))
		{
			Autoexec.Open("user://autoexec.js", 1);
			Console.Print("Autoexec loaded");
			try
			{
				Scripting.ConsoleEngine.Execute(Autoexec.GetAsText());
				Console.Print("Successfully executed autoexec");
			}
			catch(JavaScriptException Error)
			{
				Console.Print(Error.Message + " @ line " + Error.LineNumber.ToString());
				Console.Print("AUTOEXEC FAILED: Not all parts of the autoexec executed successfully. It is highly recommended that you fix your autoexec and restart the game.");
			}
		}
		else
		{
			Console.Print("Autoexec not found, creating a default one");
			System.IO.File.WriteAllText($"{OS.GetUserDataDir()}/autoexec.js", "//This is your autoexec\n//It is executed directly after command line arugments are");
		}
		Autoexec.Close();*/
	}
}
