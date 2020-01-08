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

		Bindings.Bind("W",               nameof(Player.ForwardMove));
		Bindings.Bind("LeftStickUp",     nameof(Player.ForwardMove));
		Bindings.Bind("S",               nameof(Player.BackwardMove));
		Bindings.Bind("LeftStickDown",   nameof(Player.BackwardMove));
		Bindings.Bind("D",               nameof(Player.RightMove));
		Bindings.Bind("LeftStickRight",  nameof(Player.RightMove));
		Bindings.Bind("A",               nameof(Player.LeftMove));
		Bindings.Bind("LeftStickLeft",   nameof(Player.LeftMove));
		Bindings.Bind("Alt",             nameof(Player.FlySprint));
		Bindings.Bind("LeftStickClick",  nameof(Player.FlySprint));
		Bindings.Bind("Space",           nameof(Player.Jump));
		Bindings.Bind("XboxA",           nameof(Player.Jump));
		Bindings.Bind("Shift",           nameof(Player.Crouch));
		Bindings.Bind("XboxB",           nameof(Player.Crouch));

		Bindings.Bind("WheelUp",         nameof(Player.InventoryUp));
		Bindings.Bind("XboxLB",          nameof(Player.InventoryUp));
		Bindings.Bind("WheelDown",       nameof(Player.InventoryDown));
		Bindings.Bind("XboxRB",          nameof(Player.InventoryDown));

		Bindings.Bind("1",               nameof(Player.InventorySlot0));
		Bindings.Bind("2",               nameof(Player.InventorySlot1));
		Bindings.Bind("3",               nameof(Player.InventorySlot2));
		Bindings.Bind("4",               nameof(Player.InventorySlot3));
		Bindings.Bind("5",               nameof(Player.InventorySlot4));
		Bindings.Bind("6",               nameof(Player.InventorySlot5));
		Bindings.Bind("7",               nameof(Player.InventorySlot6));
		Bindings.Bind("8",               nameof(Player.InventorySlot7));
		Bindings.Bind("9",               nameof(Player.InventorySlot8));
		Bindings.Bind("0",               nameof(Player.InventorySlot9));

		Bindings.Bind("MouseUp",         nameof(Player.LookUp));
		Bindings.Bind("RightStickUp",    nameof(Player.LookUp));
		Bindings.Bind("MouseDown",       nameof(Player.LookDown));
		Bindings.Bind("RightStickDown",  nameof(Player.LookDown));
		Bindings.Bind("MouseRight",      nameof(Player.LookRight));
		Bindings.Bind("RightStickRight", nameof(Player.LookRight));
		Bindings.Bind("MouseLeft",       nameof(Player.LookLeft));
		Bindings.Bind("RightStickLeft",  nameof(Player.LookLeft));

		Bindings.Bind("R",               nameof(Player.BuildRotate));
		Bindings.Bind("XboxX",           nameof(Player.BuildRotate));
		Bindings.Bind("Q",               nameof(Player.ThrowCurrentItem));
		Bindings.Bind("RightStickClick", nameof(Player.ThrowCurrentItem));

		Bindings.Bind("F",               nameof(Player.Interact));
		Bindings.Bind("XboxY",           nameof(Player.Interact));
		Bindings.Bind("G",               nameof(Menu.BuildInventory));
		Bindings.Bind("XboxStart",       nameof(Menu.BuildInventory));

		Bindings.Bind("K",               nameof(Player.InputRespawn));
		Bindings.Bind("XboxSelect",      nameof(Player.InputRespawn));
		Bindings.Bind("T",               nameof(Player.ToggleFly));
		Bindings.Bind("LeftStickClick",  nameof(Player.ToggleFly));

		Bindings.Bind("MouseOne",        nameof(Player.PrimaryFire));
		Bindings.Bind("XboxRT",          nameof(Player.PrimaryFire));
		Bindings.Bind("MouseTwo",        nameof(Player.SecondaryFire));
		Bindings.Bind("XboxLT",          nameof(Player.SecondaryFire));

		Bindings.Bind("P",               nameof(World.DrawPathfinderConnections));
	}
}
