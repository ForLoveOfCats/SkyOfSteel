using System;
using Godot;


public class Startup : Node
{
	//Ensures folders exist, sets up defaults, processes command line arguments and runs Autoexec.csx
	public override void _Ready()
	{
		//First we make sure that the Saves and Gamemodes folders exist
		{
			Directory Dir = new Directory();

			if(!Dir.DirExists("user://Saves"))
				Dir.MakeDir("user://Saves");

			if(!Dir.DirExists("user//Gamemodes"))
				Dir.MakeDir("user://Gamemodes");
		}


		//Then defaults are set
		SetupDefaults();

		//Then command line arguments are processed
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


		//autoexec.csx is executed last
		File Autoexec = new File();
		if(Autoexec.FileExists("user://Autoexec.csx"))
		{
			Autoexec.Open("user://Autoexec.csx", 1);
			Console.Print("Autoexec loaded");
			try
			{
				Scripting.ConsoleState = Scripting.ConsoleState.ContinueWithAsync(Autoexec.GetAsText()).Result;
				Console.Print("Successfully executed autoexec");
			}
			catch(Exception Error)
			{
				Console.Print(Error.Message);
				Console.Print("AUTOEXEC FAILED: Not all parts of the autoexec executed successfully. It is highly recommended that you fix your autoexec and restart the game.");
			}
		}
		else
		{
			Console.Print("Autoexec not found, creating a default one");
			System.IO.File.WriteAllText($"{OS.GetUserDataDir()}/Autoexec.csx", "// This is your autoexec\n// It is executed directly after command line arugments are\n\n");
		}
		Autoexec.Close();
	}


	public static void SetupDefaults()
	{
		API.FpsMax(200);
		API.ChunkRenderDistance(10);

		Bindings.Bind("W", "Game.PossessedPlayer.ForwardMove");
		Bindings.Bind("LeftStickUp", "Game.PossessedPlayer.ForwardMove");
		Bindings.Bind("S", "Game.PossessedPlayer.BackwardMove");
		Bindings.Bind("LeftStickDown", "Game.PossessedPlayer.BackwardMove");
		Bindings.Bind("D", "Game.PossessedPlayer.RightMove");
		Bindings.Bind("LeftStickRight", "Game.PossessedPlayer.RightMove");
		Bindings.Bind("A", "Game.PossessedPlayer.LeftMove");
		Bindings.Bind("LeftStickLeft", "Game.PossessedPlayer.LeftMove");
		Bindings.Bind("Shift", "Game.PossessedPlayer.Sprint");
		Bindings.Bind("LeftStickClick", "Game.PossessedPlayer.Sprint");
		Bindings.Bind("Space", "Game.PossessedPlayer.Jump");
		Bindings.Bind("XboxA", "Game.PossessedPlayer.Jump");
		Bindings.Bind("Control", "Game.PossessedPlayer.Crouch");
		Bindings.Bind("XboxB", "Game.PossessedPlayer.Crouch");

		Bindings.Bind("WheelUp", "Game.PossessedPlayer.InventoryUp");
		Bindings.Bind("XboxLB", "Game.PossessedPlayer.InventoryUp");
		Bindings.Bind("WheelDown", "Game.PossessedPlayer.InventoryDown");
		Bindings.Bind("XboxRB", "Game.PossessedPlayer.InventoryDown");

		Bindings.Bind("MouseUp", "Game.PossessedPlayer.LookUp");
		Bindings.Bind("RightStickUp", "Game.PossessedPlayer.LookUp");
		Bindings.Bind("MouseDown", "Game.PossessedPlayer.LookDown");
		Bindings.Bind("RightStickDown", "Game.PossessedPlayer.LookDown");
		Bindings.Bind("MouseRight", "Game.PossessedPlayer.LookRight");
		Bindings.Bind("RightStickRight", "Game.PossessedPlayer.LookRight");
		Bindings.Bind("MouseLeft", "Game.PossessedPlayer.LookLeft");
		Bindings.Bind("RightStickLeft", "Game.PossessedPlayer.LookLeft");

		Bindings.Bind("R", "Game.PossessedPlayer.BuildRotate");
		Bindings.Bind("XboxX", "Game.PossessedPlayer.BuildRotate");
		Bindings.Bind("Q", "Game.PossessedPlayer.DropCurrentItem");
		Bindings.Bind("RightStickClick", "Game.PossessedPlayer.DropCurrentItem");

		Bindings.Bind("K", "Game.PossessedPlayer.MovementReset");
		Bindings.Bind("XboxSelect", "Game.PossessedPlayer.MovementReset");
		Bindings.Bind("T", "Game.PossessedPlayer.ToggleFly");
		Bindings.Bind("XboxY", "Game.PossessedPlayer.ToggleFly");

		Bindings.Bind("MouseOne", "Game.PossessedPlayer.PrimaryFire");
		Bindings.Bind("XboxRT", "Game.PossessedPlayer.PrimaryFire");
		Bindings.Bind("MouseTwo", "Game.PossessedPlayer.SecondaryFire");
		Bindings.Bind("XboxLT", "Game.PossessedPlayer.SecondaryFire");
	}
}
