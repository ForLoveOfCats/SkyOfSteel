using Godot;
using System;
using System.Collections.Generic;


public class Events : Node
{
	public enum TYPE {LOCAL_PLAYER_MOVE, LOCAL_PLAYER_ROT};

	private static Events Self;
	Events()
	{
		Self = this;
	}


	public static void Run(TYPE Event, object[] Args)
	{
		switch(Event)
		{
			case(TYPE.LOCAL_PLAYER_MOVE):{
				Game.PlayerList[ (int)(Args[0]) ].SetTranslation( (Vector3)(Args[1]) );
				return;
			}


			case(TYPE.LOCAL_PLAYER_ROT):{
				Game.PlayerList[ (int)(Args[0]) ].SetRotationDegrees(new Vector3(0, (float)Args[1], 0));
				return;
			}

			default:
				throw new System.ArgumentException("Invalid event type '" + Event.ToString() + "'");
		}
	}
}
