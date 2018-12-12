using Godot;
using System;


public class Message : Node
{
	public static void NetRemoveSync(string Name)
	{
		foreach(int ClientId in Net.PeerList)
		{
			if(ClientId != 1)
			{
				Net.SendMessage(ClientId, Net.MESSAGE.REMOVE_SYNC, new object[] {Name});
			}
		}
	}
}
