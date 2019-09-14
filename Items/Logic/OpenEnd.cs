using Godot;
using static System.Diagnostics.Debug;



public class OpenEnd : StaticBody
{
	public IPipe Parent { get; private set;}


	public override void _Ready()
	{
		Assert(GetParent() is IPipe);

		Parent = GetParent() as IPipe;
	}
}
