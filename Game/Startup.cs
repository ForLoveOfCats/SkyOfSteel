using System;
using Godot;


public class Startup : Node
{
	//Ensures folders exist, sets up defaults, processes command line arguments and runs Autoexec.csx
	public override void _Ready()
	{
		//First we make sure that the Saves folder exists
		{
			Directory Dir = new Directory();

			if(!Dir.DirExists("user://Saves"))
				Dir.MakeDir("user://Saves");
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
			Console.LogLabel.Text += "\n";
		}

		Console.Print("The console is disabled in 0.1.6");
	}


	public static void SetupDefaults()
	{
		API.FpsMax(200);
		API.ChunkRenderDistance(10);

		Bindings.Bind("W", "ForwardMove");
		Bindings.Bind("LeftStickUp", "ForwardMove");
		Bindings.Bind("S", "BackwardMove");
		Bindings.Bind("LeftStickDown", "BackwardMove");
		Bindings.Bind("D", "RightMove");
		Bindings.Bind("LeftStickRight", "RightMove");
		Bindings.Bind("A", "LeftMove");
		Bindings.Bind("LeftStickLeft", "LeftMove");
		Bindings.Bind("Alt", "FlySprint");
		Bindings.Bind("LeftStickClick", "FlySprint");
		Bindings.Bind("Space", "Jump");
		Bindings.Bind("XboxA", "Jump");
		Bindings.Bind("Shift", "Crouch");
		Bindings.Bind("XboxB", "Crouch");

		Bindings.Bind("WheelUp", "InventoryUp");
		Bindings.Bind("XboxLB", "InventoryUp");
		Bindings.Bind("WheelDown", "InventoryDown");
		Bindings.Bind("XboxRB", "InventoryDown");

		Bindings.Bind("1", "InventorySlot0");
		Bindings.Bind("2", "InventorySlot1");
		Bindings.Bind("3", "InventorySlot2");
		Bindings.Bind("4", "InventorySlot3");
		Bindings.Bind("5", "InventorySlot4");
		Bindings.Bind("6", "InventorySlot5");
		Bindings.Bind("7", "InventorySlot6");
		Bindings.Bind("8", "InventorySlot7");
		Bindings.Bind("9", "InventorySlot8");
		Bindings.Bind("0", "InventorySlot9");

		Bindings.Bind("MouseUp", "LookUp");
		Bindings.Bind("RightStickUp", "LookUp");
		Bindings.Bind("MouseDown", "LookDown");
		Bindings.Bind("RightStickDown", "LookDown");
		Bindings.Bind("MouseRight", "LookRight");
		Bindings.Bind("RightStickRight", "LookRight");
		Bindings.Bind("MouseLeft", "LookLeft");
		Bindings.Bind("RightStickLeft", "LookLeft");

		Bindings.Bind("R", "BuildRotate");
		Bindings.Bind("XboxX", "BuildRotate");
		Bindings.Bind("Q", "ThrowCurrentItem");
		Bindings.Bind("RightStickClick", "ThrowCurrentItem");

		Bindings.Bind("G", "BuildInventory");
		Bindings.Bind("XboxStart", "BuildInventory");

		Bindings.Bind("K", "InputRespawn");
		Bindings.Bind("XboxSelect", "InputRespawn");
		Bindings.Bind("T", "ToggleFly");
		Bindings.Bind("XboxY", "ToggleFly");

		Bindings.Bind("MouseOne", "PrimaryFire");
		Bindings.Bind("XboxRT", "PrimaryFire");
		Bindings.Bind("MouseTwo", "SecondaryFire");
		Bindings.Bind("XboxLT", "SecondaryFire");
	}
}
