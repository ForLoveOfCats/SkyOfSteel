using Godot;


public class ExplodeSfx : AudioStreamPlayer3D
{
	public static float MaxLife = 2; //In seconds

	public float Life = 0;


	public override void _Process(float Delta)
	{
		Life += Delta;

		if(Life >= MaxLife)
			QueueFree();
	}
}
