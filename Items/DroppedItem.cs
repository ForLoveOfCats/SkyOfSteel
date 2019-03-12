using Godot;
using System;


public class DroppedItem : KinematicBody
{
	private const float Gravity = 14f;
	private const float MaxFallSpeed = -40f;
	private const float RPS = 0.5f; //Revolutions Per Second

	public Vector3 Momentum; //Needs to be set when created or else will crash with NullReferenceException
	private bool PhysicsEnabled = true;
	public Items.TYPE Type;

	public override void _Ready()
	{
		ShaderMaterial Mat = new ShaderMaterial();
		Mat.Shader = Items.StructureShader;
		Mat.SetShaderParam("texture_albedo", Items.Textures[Type]);
		GetNode<MeshInstance>("MeshInstance").MaterialOverride = Mat;
	}


	public void Remove()
	{
		World.DroppedItems.Remove(this);
	}


	public override void _PhysicsProcess(float Delta)
	{
		SetRotationDegrees(new Vector3(0, RotationDegrees.y+(360*Delta*RPS), 0));

		if(PhysicsEnabled)
		{
			Momentum = MoveAndSlide(Momentum, new Vector3(0,1,0));

			PhysicsEnabled = !IsOnFloor();
			if(PhysicsEnabled)
			{
				Momentum.y -= Gravity*Delta;
				if(Momentum.y < MaxFallSpeed)
				{
					Momentum.y = MaxFallSpeed;
				}
			}
			else
			{
				Momentum = new Vector3(0,0,0);
			}
		}
	}
}
