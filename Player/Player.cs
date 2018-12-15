using Godot;
using System;


public class Player : KinematicBody
{
	public bool Possessed = false;
	public int Id = 0;

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
	public bool IsSprinting = false;
	public bool IsJumping = false;
	private float JumpTimer = 0f;
	private Vector3 Momentum = new Vector3(0,0,0);
	private float LookHorizontal = 0;
	private float LookVertical = 0;
	private bool IsPrimaryFiring = false;
	private bool IsSecondaryFiring = false;

	public double ForwardSens = 0d;
	public double BackwardSens = 0d;
	public double RightSens = 0d;
	public double LeftSens = 0d;

	public Items.Instance[] Inventory = new Items.Instance[10];
	public int InventorySlot = 0;

	private HUD HUDInstance;

	private Ghost GhostInstance;

	Player()
	{
		ItemGive(new Items.Instance(Items.TYPE.PLATFORM));
		ItemGive(new Items.Instance(Items.TYPE.WALL));
		ItemGive(new Items.Instance(Items.TYPE.SLOPE));
	}


	public override void _Ready()
	{
		Translation = new Vector3(0,1,0);
		if(Possessed)
		{
			GetNode<Camera>("SteelCamera").MakeCurrent();
			GetNode<MeshInstance>("FPSMesh").Hide();
			HUDInstance = ((PackedScene)GD.Load("res://UI/HUD.tscn")).Instance() as HUD;
			AddChild(HUDInstance);

			GhostInstance = ((PackedScene)(GD.Load("res://Building/Ghost.tscn"))).Instance() as Ghost;
			GetParent().AddChild(GhostInstance);
			GhostInstance.Hide();
		}
		else
		{
			SetProcess(false);
		}
	}


	public void ItemGive(Items.Instance ToGive)
	{
		for(int Slot = 0; Slot <= 9; Slot++)
		{
			if(!(Inventory[Slot] is null)) //If inventory item is not null
			{
				if(Inventory[Slot].Type == ToGive.Type)
				{
					Inventory[Slot].Count += ToGive.Count;
					return;
				}
			}
		}

		for(int Slot = 0; Slot <= 9; Slot++)
		{
			if(Inventory[Slot] is null)
			{
				Inventory[Slot] = ToGive;
				return;
			}
		}
	}


	public void InventoryUp()
	{
		InventorySlot--;
		if(InventorySlot < 0)
		{
			InventorySlot = 9;
		}
		HUDInstance.HotbarUpdate();
	}


	public void InventoryDown()
	{
		InventorySlot++;
		if(InventorySlot > 9)
		{
			InventorySlot = 0;
		}
		HUDInstance.HotbarUpdate();
	}


	public void ForwardMove(double Sens)
	{
		ForwardSens = Sens;
		if(Sens > 0d)
		{
			BackwardSens = 0d;
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
		BackwardSens = Sens;
		if(Sens > 0d)
		{
			ForwardSens = 0d;
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
		RightSens = Sens;
		if(Sens > 0d)
		{
			LeftSens = 0d;
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
		LeftSens = Sens;
		if(Sens > 0d)
		{
			RightSens = 0d;
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
			float Change = ((float)Sens/LookDivisor)*Game.MouseSensitivity;

			if(ShouldDo.LocalPlayerPitch(Change))
			{
				LookVertical = Mathf.Clamp(LookVertical+Change, -90, 90);
				GetNode<Camera>("SteelCamera").SetRotationDegrees(new Vector3(LookVertical, 180, 0));
			}
		}
	}


	public void LookDown(double Sens)
	{
		if(Sens > 0d)
		{
			float Change = ((float)Sens/LookDivisor)*Game.MouseSensitivity;

			if(ShouldDo.LocalPlayerPitch(-Change))
			{
				LookVertical = Mathf.Clamp(LookVertical-Change, -90, 90);
				GetNode<Camera>("SteelCamera").SetRotationDegrees(new Vector3(LookVertical, 180, 0));
			}
		}
	}


	public void LookRight(double Sens)
	{
		if(Sens > 0d)
		{
			float Change = ((float)Sens/LookDivisor)*Game.MouseSensitivity;

			if(ShouldDo.LocalPlayerRotate(-Change))
			{
				LookHorizontal -= Change;
				SetRotationDegrees(new Vector3(0, LookHorizontal, 0));
			}
		}
	}


	public void LookLeft(double Sens)
	{
		if(Sens > 0d)
		{
			float Change = ((float)Sens/LookDivisor)*Game.MouseSensitivity;

			if(ShouldDo.LocalPlayerRotate(+Change))
			{
				LookHorizontal += Change;
				SetRotationDegrees(new Vector3(0, LookHorizontal, 0));
			}
		}
	}


	public void PrimaryFire(double Sens)
	{
		if(Sens > 0d && !IsPrimaryFiring)
		{
			IsPrimaryFiring = true;

			if(Inventory[InventorySlot] != null)
			{
				//Assume for now that all primary fire opertations are to build
				RayCast BuildRayCast = GetNode("SteelCamera/RayCast") as RayCast;
				if(BuildRayCast.IsColliding())
				{
					Structure Hit = BuildRayCast.GetCollider() as Structure;
					if(Hit != null && GhostInstance.CanBuild)
					{
						Building.PlaceOn(Hit, Inventory[InventorySlot].Type, 1);
						//ID 1 for now so all client own all non-default structures
					}
				}
			}
		}
		if(Sens <= 0d && IsPrimaryFiring)
		{
			IsPrimaryFiring = false;
		}
	}


	public void SecondaryFire(double Sens)
	{
		if(Sens > 0d && !IsSecondaryFiring)
		{
			IsSecondaryFiring = true;

			//Assume for now that all secondary fire opertations are to remove
			RayCast BuildRayCast = GetNode("SteelCamera/RayCast") as RayCast;
			if(BuildRayCast.IsColliding())
			{
				Structure Hit = BuildRayCast.GetCollider() as Structure;
				if(Hit != null)
				{
					// Message.NetRemoveRequest(Hit.Name);
					//Name is GUID used to reference individual structures over network
					Hit.Remove();
				}
			}
		}
		if(Sens <= 0d && IsSecondaryFiring)
		{
			IsSecondaryFiring = false;
		}
	}


	public override void _PhysicsProcess(float Delta)
	{
		if(!Possessed)
		{
			return;
		}

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
		//MoveAndSlide multiplies by *physics* delta internally
		Vector3 NewPos = Translation;
		Translation = OldPos;
		if(NewPos != OldPos)
		{
			if(ShouldDo.LocalPlayerMove(NewPos))
			{
				Translation = NewPos;
			}
		}

		if(IsOnFloor() && Momentum.y <= 0f)
		{
			Momentum.y = -1f;
		}

		RpcUnreliable(nameof(Update), Translation, RotationDegrees);
	}


	[Remote]
	public void Update(Vector3 Position, Vector3 Rotation)
	{
		if(ShouldDo.RemotePlayerMove(Id, Position))
		{
			Translation = Position;
		}

		if(ShouldDo.RemotePlayerRotate(Id, Rotation))
		{
			RotationDegrees = Rotation;
		}
	}
}
