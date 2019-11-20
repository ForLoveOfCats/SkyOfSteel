using Godot;



public class Scripting : Node
{
	public static Scripting Self;
	Scripting()
	{
		if(Engine.EditorHint) {return;}

		Self = this;
	}


	public static void RunConsoleLine(string Line)
	{}
}
