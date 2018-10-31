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
				Game.PlayerList[Self.GetTree().GetNetworkUniqueId()].SetTranslation( (Vector3)(Args[0]) );
				return;
			}


			case(TYPE.LOCAL_PLAYER_ROT):{
				Game.PlayerList[Self.GetTree().GetNetworkUniqueId()].SetRotationDegrees(new Vector3(0, (float)Args[0], 0));
				return;
			}

			default:
				throw new System.ArgumentException("Invalid event type '" + Event.ToString() + "'");
		}
	}
}
