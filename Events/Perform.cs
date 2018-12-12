using Godot;
using System;


public class Perform : Node
{
	public static void Remove(Events.INVOKER Invoker, string Name)
	{
		Events.Run(new EventObject(Invoker, Events.TYPE.REMOVE, new object[] {Name}));
	}
}
