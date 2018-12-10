using Godot;
using System;
using System.Collections.Generic;


public class Events : Node
{
	public enum TYPE {PLACE_REQUEST, PLACE, REMOVE_REQUEST, REMOVE};
	public enum INVOKER {CLIENT, SERVER};

	private static Events Self;
	Events()
	{
		Self = this;
	}


	public static void Run(EventObject EventArg)
	{
		switch(EventArg.Type)
		{
			case(TYPE.PLACE_REQUEST):{
				Message.NetPlaceRequest((int)EventArg.Args[0], (Items.TYPE)EventArg.Args[1], (Vector3)EventArg.Args[2], (Vector3)EventArg.Args[3]);
				return;
			}


			case(TYPE.PLACE):{
				Building.Place((Items.TYPE)EventArg.Args[1], (Vector3)EventArg.Args[2], (Vector3)EventArg.Args[3], (int)EventArg.Args[0], (string)EventArg.Args[4]);
				return;
			}


			case(TYPE.REMOVE):{
				Building.Remove((string)EventArg.Args[0]);
				if(Self.GetTree().IsNetworkServer())
				{
					Message.NetRemoveSync((string)EventArg.Args[0]);
				}
				return;
			}


			default:
				throw new System.ArgumentException("Invalid event type '" + EventArg.Type.ToString() + "'");
		}
	}
}
