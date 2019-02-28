using Godot;
using System;


public class DroppedItem : KinematicBody
{
	private const float RPS = 0.5f; //Revolutions Per Second

	public Vector3 Momentum; //Needs to be set when created or else will crash with NullReferenceException
	private bool PhysicsEnabled = true;
	public Items.TYPE Type = Items.TYPE.ERROR;

	public override void _Ready()
	{
		ShaderMaterial Mat = new ShaderMaterial();
		Mat.Shader = Items.StructureShader;
		Mat.SetShaderParam("texture_albedo", Items.Textures[Type]);
		GetNode<MeshInstance>("MeshInstance").MaterialOverride = Mat;
	}


	public override void _PhysicsProcess(float Delta)
	{
		SetRotationDegrees(new Vector3(0, RotationDegrees.y+(360*Delta*RPS), 0));
		if(PhysicsEnabled)
		{
			PhysicsEnabled = (MoveAndCollide(Momentum*Delta) == null);
		}
	}
}
