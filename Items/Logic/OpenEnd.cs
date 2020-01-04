using Godot;



public class OpenEnd : StaticBody
{
	public PipeCoreLogic Parent { get; private set;}


	public override void _Ready()
	{
		Assert.ActualAssert(GetParent() is PipeCoreLogic);

		Parent = GetParent() as PipeCoreLogic;
	}
}
