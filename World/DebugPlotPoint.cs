using Godot;



public class DebugPlotPoint : MeshInstance
{
	public float MaxLife = 0;

	private float CurrentLife;


	public override void _Process(float Delta)
	{
		CurrentLife += Delta;

		if(MaxLife > 0 && CurrentLife > MaxLife)
			QueueFree();
	}
}
