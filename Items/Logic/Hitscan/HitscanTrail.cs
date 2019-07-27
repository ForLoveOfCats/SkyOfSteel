using Godot;
using static Godot.Mathf;


public class HitscanTrail : MeshInstance
{
	public static float MaxLife = 0.5f; //In seconds

	public float Life = 0;
	public ShaderMaterial Mat;

	private static Shader TrailShader;

	static HitscanTrail()
	{
		TrailShader = GD.Load<Shader>("res://Items/Logic/Hitscan/HitscanTrailShader.shader");
	}


	public override void _Ready()
	{
		Mat = new ShaderMaterial();
		Mat.Shader = TrailShader;
		Mat.SetShaderParam("alpha", 1);
		MaterialOverride = Mat;
	}


	public override void _Process(float Delta)
	{
		Life += Delta;
		if(Life >= MaxLife)
			QueueFree();

		//This is probably rather inefficient
		Mat.SetShaderParam("alpha", Clamp(1 - (Life/MaxLife), 0, 1));
	}


	public void ApplyLength(float Length) //Scale gets reset after _Ready so we CallDeferred this to get around it
	{
		Scale = new Vector3(Scale.x, Scale.y, Length);
	}
}

