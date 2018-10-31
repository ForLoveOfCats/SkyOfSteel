using Godot;
using System;
using System.Collections.Generic;


public class Events : Node
{
	public enum TYPE {LOCAL_PLAYER_MOVE, LOCAL_PLAYER_ROT};
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
			case(TYPE.LOCAL_PLAYER_MOVE):{
				Game.PossessedPlayer.SetTranslation( (Vector3)(EventArg.Args[0]) );
				return;
			}


			case(TYPE.LOCAL_PLAYER_ROT):{
				Game.PossessedPlayer.SetRotationDegrees(new Vector3(0, (float)EventArg.Args[0], 0));
				return;
			}

			default:
				throw new System.ArgumentException("Invalid event type '" + EventArg.Type.ToString() + "'");
		}
	}
}
