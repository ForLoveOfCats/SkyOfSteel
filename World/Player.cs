using Godot;
using System;


public class Player : KinematicBody
{
	public bool Possessed = false;

	private const float BaseMovementSpeed = 16;
	private const float MovementInputMultiplyer = BaseMovementSpeed;
	private const float SprintMultiplyer = 2;
	private const float MaxMovementSpeed = BaseMovementSpeed*SprintMultiplyer;
	private const float Friction = BaseMovementSpeed*10;
	private const float JumpStartForce = 8f;
	private const float JumpContinueForce = 6f;
	private const float MaxJumpLength = 0.3f;
	private const float Gravity = 14f;
	private const float LookDivisor = 6;

	private int ForwardAxis = 0;
	private int RightAxis = 0;
	private bool IsSprinting = false;
	private bool IsJumping = false;
	private float JumpTimer = 0f;
	private Vector3 Momentum = new Vector3(0,0,0);
	private float LookHorizontal = 0;
	private float LookVertical = 0;


	public override void _Ready()
	{
		Translation = new Vector3(0,1,0);
		if(Possessed)
		{
			((Camera)GetNode("SteelCamera")).MakeCurrent();
			((MeshInstance)GetNode("FPSMesh")).Hide();
			AddChild(((PackedScene)GD.Load("res://UI/SteelHUD.tscn")).Instance());
		}
		else
		{
			SetProcess(false);
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


	public void Jump(double Sens)
	{
		if(Sens > 0d)
		{
			if(IsOnFloor())
			{
				Momentum.y = JumpStartForce;
				IsJumping = true;
			}
		}
		else
		{
			IsJumping = false;
		}
	}


	public void LookUp(double Sens)
	{
		if(Sens > 0d)
		{
			LookVertical = Mathf.Clamp(LookVertical+((float)Sens/LookDivisor)*Game.MouseSensitivity, -90, 90);
			GetNode<Camera>("SteelCamera").SetRotationDegrees(new Vector3(LookVertical, 180, 0));
		}
	}


	public void LookDown(double Sens)
	{
		if(Sens > 0d)
		{
			LookVertical = Mathf.Clamp(LookVertical-((float)Sens/LookDivisor)*Game.MouseSensitivity, -90, 90);
			GetNode<Camera>("SteelCamera").SetRotationDegrees(new Vector3(LookVertical, 180, 0));
		}
	}


	public void LookRight(double Sens)
	{
		LookHorizontal -= ((float)Sens/LookDivisor)*Game.MouseSensitivity;
		if(LookHorizontal < 0)
		{
			LookHorizontal = 360+LookHorizontal;
		}

		Perform.LocalPlayerRotate(Events.INVOKER.CLIENT, LookHorizontal);
	}


	public void LookLeft(double Sens)
	{
		LookHorizontal += ((float)Sens/LookDivisor)*Game.MouseSensitivity;
		if(LookHorizontal > 360)
		{
			LookHorizontal = LookHorizontal-360;
		}

		Perform.LocalPlayerRotate(Events.INVOKER.CLIENT, LookHorizontal);
	}


	public override void _Process(float Delta)
	{
		if(ForwardAxis == 0 && IsOnFloor())
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

		if(RightAxis == 0 && IsOnFloor())
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

		if(IsJumping && JumpTimer <= MaxJumpLength)
		{
			JumpTimer += Delta;
			Momentum.y = Mathf.Clamp(Momentum.y+JumpContinueForce*Delta, -MaxMovementSpeed, MaxMovementSpeed);
		}
		else
		{
			JumpTimer = 0f;
			IsJumping = false;
			Momentum.y = Mathf.Clamp(Momentum.y-Gravity*Delta, -MaxMovementSpeed, MaxMovementSpeed);
		}

		Vector3 OldPos = Translation;
		MoveAndSlide(Momentum.Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(LookHorizontal)), new Vector3(0,1,0), 0.05f, 4);
		Vector3 NewPos = Translation;
		Translation = OldPos;
		if(NewPos != OldPos)
		{
			Perform.LocalPlayerMove(Events.INVOKER.CLIENT, NewPos);
		}

		if(IsOnFloor() && Momentum.y <= 0f)
		{
			Momentum.y = -1f;
		}

		Message.PlayerRequestPos(Translation);
		Message.PlayerRequestRot(RotationDegrees.y);
	}
}
