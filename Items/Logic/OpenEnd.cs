using Godot;
using static System.Diagnostics.Debug;



public class OpenEnd : StaticBody
{
	public PipeCoreLogic Parent { get; private set;}


	public override void _Ready()
	{
		Assert(GetParent() is PipeCoreLogic);

		Parent = GetParent() as PipeCoreLogic;
	}
}
