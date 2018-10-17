using Godot;
using System;
using System.Collections.Generic;


public class Events : Node
{
	public enum TYPE {PLAYER_MOVE};

	private static Events Self;
	Events()
	{
		Self = this;
	}


	public static void Run(TYPE Event, object[] Args)
	{
		switch(Event)
		{
			case(TYPE.PLAYER_MOVE):{
				Game.PlayerList[ (int)(Args[0]) ].SetTranslation( (Vector3)(Args[1]) );
				return;
			}

			default:
				throw new System.ArgumentException("Invalid event type '" + Event.ToString() + "'");
		}
	}
}
