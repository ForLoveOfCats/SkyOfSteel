using Godot;
using System;


public class Player : Spatial
{
	public bool Possessed = false;

	private const float BaseMovementSpeed = 16;
	private const float MovementInputMultiplyer = BaseMovementSpeed;
	private const float SprintMultiplyer = 2;
	private const float MaxMovementSpeed = BaseMovementSpeed*SprintMultiplyer;
	private const float Friction = BaseMovementSpeed*10;

	private int ForwardAxis = 0;
	private int RightAxis = 0;
	private bool IsSprinting = false;
	private Vector3 Momentum = new Vector3(0,0,0);

	public override void _Ready()
	{
		Translation = new Vector3(0,1,0);
		if(Possessed)
		{
			((Camera)GetNode("SteelCamera")).MakeCurrent();
			((MeshInstance)GetNode("FPSMesh")).Hide();
			AddChild(((PackedScene)GD.Load("res://UI/SteelHUD.tscn")).Instance());
		}
	}

	public void ForwardMove(double Sens)
	{
		if(Sens > 0d)
		{
			ForwardAxis = 1;
			if(IsSprinting)
			{
				Momentum.z = Mathf.Clamp((float)(Sens*MovementInputMultiplyer*SprintMultiplyer), 0f, MaxMovementSpeed);
			}
			else
			{
				Momentum.z = Mathf.Clamp((float)(Sens*MovementInputMultiplyer), 0f, BaseMovementSpeed);
			}
		}
		else if(ForwardAxis > 0)
		{
			ForwardAxis = 0;
		}
	}


	public void BackwardMove(double Sens)
	{
		if(Sens > 0d)
		{
			if(IsSprinting)
			{
				Momentum.z = Mathf.Clamp((float)(-1*Sens*MovementInputMultiplyer*SprintMultiplyer), -MaxMovementSpeed, 0f);
			}
			else
			{
				Momentum.z = Mathf.Clamp((float)(-1*Sens*MovementInputMultiplyer), -BaseMovementSpeed, 0f);
			}
			ForwardAxis = -1;
		}
		else if(ForwardAxis < 0)
		{
			ForwardAxis = 0;
		}
	}


	public void RightMove(double Sens)
	{
		if(Sens > 0d)
		{
			if(IsSprinting)
			{
				Momentum.x = Mathf.Clamp((float)(-1*Sens*MovementInputMultiplyer*SprintMultiplyer), -MaxMovementSpeed, 0f);
			}
			else
			{
				Momentum.x = Mathf.Clamp((float)(-1*Sens*MovementInputMultiplyer), -BaseMovementSpeed, 0f);
			}
			//Momentum.x = Mathf.Clamp((float)(-1*Sens*MovementInputMultiplyer), -BaseMovementSpeed, 0f);
			RightAxis = 1;
		}
		else if(RightAxis > 0)
		{
			RightAxis = 0;
		}
	}


	public void LeftMove(double Sens)
	{
		if(Sens > 0d)
		{
			if(IsSprinting)
			{
				Momentum.x = Mathf.Clamp((float)(Sens*MovementInputMultiplyer*SprintMultiplyer), 0f, MaxMovementSpeed);
			}
			else
			{
				Momentum.x = Mathf.Clamp((float)(Sens*MovementInputMultiplyer), 0f, BaseMovementSpeed);
			}
			//Momentum.x = Mathf.Clamp((float)(Sens*MovementInputMultiplyer), 0f, BaseMovementSpeed);
			RightAxis = -1;
		}
		else if(RightAxis < 0)
		{
			RightAxis = 0;
		}
	}


	public void Sprint(double Sens)
	{
		if(Sens > 0d)
		{
			IsSprinting = true;
			if(ForwardAxis != 0)
			{
				Momentum.z = Momentum.z*SprintMultiplyer;
			}

			if(RightAxis != 0)
			{
				Momentum.x = Momentum.x*SprintMultiplyer;
			}
		}
		else
		{
			IsSprinting = false;
			Momentum.z = Mathf.Clamp(Momentum.z, -BaseMovementSpeed, BaseMovementSpeed);
			Momentum.x = Mathf.Clamp(Momentum.x, -BaseMovementSpeed, BaseMovementSpeed);
		}
	}


	public override void _Process(float Delta)
	{
		if(ForwardAxis == 0)
		{
			if(Momentum.z > 0)
			{
				Momentum.z = Mathf.Clamp(Momentum.z-Friction*Delta, 0f, MaxMovementSpeed);
			}
			else if (Momentum.z < 0)
			{
				Momentum.z = Mathf.Clamp(Momentum.z+Friction*Delta, -MaxMovementSpeed, 0f);
			}
		}

		if(RightAxis == 0)
		{
			if(Momentum.x > 0)
			{
				Momentum.x = Mathf.Clamp(Momentum.x-Friction*Delta, 0f, MaxMovementSpeed);
			}
			else if (Momentum.x < 0)
			{
				Momentum.x = Mathf.Clamp(Momentum.x+Friction*Delta, -MaxMovementSpeed, 0f);
			}
		}

		Translate(Momentum*Delta);
	}
}
