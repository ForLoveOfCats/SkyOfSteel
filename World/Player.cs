using Godot;
using System;


public class Player : Spatial
{
	public bool Possessed = false;

	private const float BaseMovementSpeed = 16;
	private const float MovementInputMultiplyer = BaseMovementSpeed;
	private const float MaxMovementSpeed = 90;
	private const float Friction = BaseMovementSpeed*10;

	private int ForwardAxis = 0;
	private int RightAxis = 0;
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
		if(!Game.PlayerInputEnabled)
		{
			return;
		}

		if(Sens > 0d)
		{
			Momentum.z = Mathf.Clamp((float)(Sens*MovementInputMultiplyer), 0f, BaseMovementSpeed);
			ForwardAxis = 1;
		}
		else if(ForwardAxis > 0)
		{
			ForwardAxis = 0;
		}
	}


	public void BackwardMove(double Sens)
	{
		if(!Game.PlayerInputEnabled)
		{
			return;
		}

		if(Sens > 0d)
		{
			Momentum.z = Mathf.Clamp((float)(-1*Sens*MovementInputMultiplyer), -BaseMovementSpeed, 0f);
			ForwardAxis = -1;
		}
		else if(ForwardAxis < 0)
		{
			ForwardAxis = 0;
		}
	}


	public void RightMove(double Sens)
	{
		if(!Game.PlayerInputEnabled)
		{
			return;
		}

		if(Sens > 0d)
		{
			Momentum.x = Mathf.Clamp((float)(-1*Sens*MovementInputMultiplyer), -BaseMovementSpeed, 0f);
			RightAxis = 1;
		}
		else if(RightAxis > 0)
		{
			RightAxis = 0;
		}
	}


	public void LeftMove(double Sens)
	{
		if(!Game.PlayerInputEnabled)
		{
			return;
		}

		if(Sens > 0d)
		{
			Momentum.x = Mathf.Clamp((float)(Sens*MovementInputMultiplyer), 0f, BaseMovementSpeed);
			RightAxis = -1;
		}
		else if(RightAxis < 0)
		{
			RightAxis = 0;
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

		GD.Print(Momentum);
	}
}
