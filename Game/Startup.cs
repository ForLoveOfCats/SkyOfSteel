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
			Console.LogLabel.Text += "\n";
	}


	public static void SetupDefaults()
	{
		API.FpsMax(200);
		API.ChunkRenderDistance(10);

		Bindings.Bind("W",               nameof(PlayerInput.ForwardMove));
		Bindings.Bind("LeftStickUp",     nameof(PlayerInput.ForwardMove));
		Bindings.Bind("S",               nameof(PlayerInput.BackwardMove));
		Bindings.Bind("LeftStickDown",   nameof(PlayerInput.BackwardMove));
		Bindings.Bind("D",               nameof(PlayerInput.RightMove));
		Bindings.Bind("LeftStickRight",  nameof(PlayerInput.RightMove));
		Bindings.Bind("A",               nameof(PlayerInput.LeftMove));
		Bindings.Bind("LeftStickLeft",   nameof(PlayerInput.LeftMove));
		Bindings.Bind("Alt",             nameof(PlayerInput.FlySprint));
		Bindings.Bind("LeftStickClick",  nameof(PlayerInput.FlySprint));
		Bindings.Bind("Space",           nameof(PlayerInput.Jump));
		Bindings.Bind("XboxA",           nameof(PlayerInput.Jump));
		Bindings.Bind("Shift",           nameof(PlayerInput.Crouch));
		Bindings.Bind("XboxB",           nameof(PlayerInput.Crouch));

		Bindings.Bind("WheelUp",         nameof(PlayerInput.InventoryUp));
		Bindings.Bind("XboxLB",          nameof(PlayerInput.InventoryUp));
		Bindings.Bind("WheelDown",       nameof(PlayerInput.InventoryDown));
		Bindings.Bind("XboxRB",          nameof(PlayerInput.InventoryDown));

		Bindings.Bind("1",               nameof(PlayerInput.InventorySlot0));
		Bindings.Bind("2",               nameof(PlayerInput.InventorySlot1));
		Bindings.Bind("3",               nameof(PlayerInput.InventorySlot2));
		Bindings.Bind("4",               nameof(PlayerInput.InventorySlot3));
		Bindings.Bind("5",               nameof(PlayerInput.InventorySlot4));
		Bindings.Bind("6",               nameof(PlayerInput.InventorySlot5));
		Bindings.Bind("7",               nameof(PlayerInput.InventorySlot6));
		Bindings.Bind("8",               nameof(PlayerInput.InventorySlot7));
		Bindings.Bind("9",               nameof(PlayerInput.InventorySlot8));
		Bindings.Bind("0",               nameof(PlayerInput.InventorySlot9));

		Bindings.Bind("MouseUp",         nameof(PlayerInput.LookUp));
		Bindings.Bind("RightStickUp",    nameof(PlayerInput.LookUp));
		Bindings.Bind("MouseDown",       nameof(PlayerInput.LookDown));
		Bindings.Bind("RightStickDown",  nameof(PlayerInput.LookDown));
		Bindings.Bind("MouseRight",      nameof(PlayerInput.LookRight));
		Bindings.Bind("RightStickRight", nameof(PlayerInput.LookRight));
		Bindings.Bind("MouseLeft",       nameof(PlayerInput.LookLeft));
		Bindings.Bind("RightStickLeft",  nameof(PlayerInput.LookLeft));

		Bindings.Bind("R",               nameof(PlayerInput.BuildRotate));
		Bindings.Bind("XboxX",           nameof(PlayerInput.BuildRotate));
		Bindings.Bind("Q",               nameof(PlayerInput.ThrowCurrentItem));
		Bindings.Bind("RightStickClick", nameof(PlayerInput.ThrowCurrentItem));

		Bindings.Bind("F",               nameof(PlayerInput.Interact));
		Bindings.Bind("XboxY",           nameof(PlayerInput.Interact));
		Bindings.Bind("G",               nameof(Menu.BuildInventory));
		Bindings.Bind("XboxStart",       nameof(Menu.BuildInventory));

		Bindings.Bind("K",               nameof(PlayerInput.InputReset));
		Bindings.Bind("XboxSelect",      nameof(PlayerInput.InputReset));
		Bindings.Bind("T",               nameof(PlayerInput.ToggleFly));
		Bindings.Bind("LeftStickClick",  nameof(PlayerInput.ToggleFly));

		Bindings.Bind("MouseOne",        nameof(PlayerInput.PrimaryFire));
		Bindings.Bind("XboxRT",          nameof(PlayerInput.PrimaryFire));
		Bindings.Bind("MouseTwo",        nameof(PlayerInput.SecondaryFire));
		Bindings.Bind("XboxLT",          nameof(PlayerInput.SecondaryFire));

		Bindings.Bind("P",               nameof(World.DrawPathfinderConnections));
	}
}
