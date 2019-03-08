using Godot;
using static Godot.Mathf;
using static SteelMath;
using System;


public class Player : KinematicBody
{
	public bool Possessed = false;
	public int Id = 0;

	private const float BaseMovementSpeed = 16;
	private const float MovementInputMultiplyer = BaseMovementSpeed;
	private const float SprintMultiplyer = 2;
	private const float MaxMovementSpeed = BaseMovementSpeed*SprintMultiplyer;
	private const float AirAcceleration = 24; //How many units per second to accelerate
	private const float Friction = BaseMovementSpeed*10;
	private const float JumpSpeedMultiplyer = 1.2f;
	private const float JumpStartForce = 8f;
	private const float JumpContinueForce = 6f;
	private const float MaxJumpLength = 0.3f;
	private const float Gravity = 14f;
	private const float ItemThrowPower = 15f;
	private const float LookDivisor = 6;

	private bool Frozen = true;
	public bool FlyMode { get; private set;} = false;

	public System.Tuple<int, int> CurrentChunk = new System.Tuple<int, int>(0, 0);

	private int ForwardAxis = 0;
	private int RightAxis = 0;
	private int JumpAxis = 0;
	public bool IsCrouching = false;
	public bool IsSprinting = false;
	public bool IsJumping = false;
	public bool WasOnFloor = false;
	private float JumpTimer = 0f;
	private Vector3 Momentum = new Vector3(0,0,0);
	private float LookHorizontal = 0;
	private float LookVertical = 0;
	private bool IsPrimaryFiring = false;
	private bool IsSecondaryFiring = false;

	public float ForwardSens = 0;
	public float BackwardSens = 0;
	public float RightSens = 0;
	public float LeftSens = 0;
	public float SprintSens = 0;
	public float JumpSens = 0;

	public Items.Instance[] Inventory = new Items.Instance[10];
	public int InventorySlot = 0;

	public int BuildRotation = 0;

	public Camera Cam;

	public HUD HUDInstance;
	private Ghost GhostInstance;

	Player()
	{
		if(Engine.EditorHint) {return;}

		ItemGive(new Items.Instance(Items.TYPE.PLATFORM));
		ItemGive(new Items.Instance(Items.TYPE.WALL));
		ItemGive(new Items.Instance(Items.TYPE.SLOPE));

		HUDInstance = ((PackedScene)GD.Load("res://UI/HUD.tscn")).Instance() as HUD;
	}


	public override void _Ready()
	{
		Cam = GetNode<Camera>("SteelCamera");

		PositionReset();

		if(Possessed)
		{
			GetNode<Camera>("SteelCamera").MakeCurrent();

			GetNode<RayCast>("SteelCamera/RayCast").AddException(this);

			GetNode<MeshInstance>("FPSMesh").Hide();

			AddChild(HUDInstance);

			GhostInstance = ((PackedScene)(GD.Load("res://Building/Ghost.tscn"))).Instance() as Ghost;
			GhostInstance.Hide();
			GetParent().CallDeferred("add_child", GhostInstance);
		}
		else
		{
			SetProcess(false);
			return;
		}

		if(GetTree().IsNetworkServer())
		{
			SetFreeze(false);
		}
	}


	[Remote]
	public void SetFreeze(bool NewFrozen)
	{
		if(GetName() == GetTree().GetNetworkUniqueId().ToString())
		{
			Frozen = NewFrozen;
		}
		else
		{
			int Id = 0;
			Int32.TryParse(GetName(), out Id);
			RpcId(Id, nameof(SetFreeze), NewFrozen);
		}
	}


	public void SetFly(bool NewFly) //because custom setters are weird
	{
		FlyMode = NewFly;
		Momentum = new Vector3(0,0,0);
	}


	public void PositionReset()
	{
		Translation = new Vector3(0,1,0);
	}


	private Vector3 AirAccelerate(Vector3 Vel, Vector3 WishDir, float Delta)
	{
		float CurrentSpeed = Vel.Dot(WishDir);
		float AddSpeed = MaxMovementSpeed - CurrentSpeed;
		AddSpeed = Clamp(AddSpeed, 0, AirAcceleration*Delta);
		return Vel + WishDir * AddSpeed;
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
		BuildRotation = 0;

		InventorySlot--;
		if(InventorySlot < 0)
		{
			InventorySlot = 9;
		}

		if(HUDInstance != null)
		{
			HUDInstance.HotbarUpdate();
		}
	}


	public void InventoryDown()
	{
		BuildRotation = 0;

		InventorySlot++;
		if(InventorySlot > 9)
		{
			InventorySlot = 0;
		}

		if(HUDInstance != null)
		{
			HUDInstance.HotbarUpdate();
		}
	}


	public void BuildRotate(float Sens)
	{
		if(Sens > 0 && Inventory[InventorySlot] != null)
		{
			switch(Inventory[InventorySlot].Type)
			{
				case(Items.TYPE.SLOPE):
					if(BuildRotation == 0)
					{
						BuildRotation = 1;
					}
					else
					{
						BuildRotation = 0;
					}
					break;
			}
		}
	}


	public void ForwardMove(float Sens)
	{
		if(ShouldDo.LocalPlayerForward(Sens))
		{
			ForwardSens = Sens;
			if(Sens > 0)
			{
				BackwardSens = 0;
				ForwardAxis = 1;

				if((IsOnFloor() && JumpAxis < 1) || FlyMode)
				{
					if(IsSprinting)
					{
						Momentum.z = Mathf.Clamp((float)(Sens*MovementInputMultiplyer*SprintMultiplyer), 0f, MaxMovementSpeed);
					}
					else
					{
						Momentum.z = Mathf.Clamp((float)(Sens*MovementInputMultiplyer), 0f, BaseMovementSpeed);
					}
				}
			}
			else if(ForwardAxis > 0)
			{
				ForwardAxis = 0;
			}
		}
	}


	public void BackwardMove(float Sens)
	{
		if(ShouldDo.LocalPlayerBackward(Sens))
		{
			BackwardSens = Sens;
			if(Sens > 0)
			{
				ForwardSens = 0;
				ForwardAxis = -1;

				if((IsOnFloor() && JumpAxis < 1) || FlyMode)
				{
					if(IsSprinting)
					{
						Momentum.z = Mathf.Clamp((float)(-1*Sens*MovementInputMultiplyer*SprintMultiplyer), -MaxMovementSpeed, 0f);
					}
					else
					{
						Momentum.z = Mathf.Clamp((float)(-1*Sens*MovementInputMultiplyer), -BaseMovementSpeed, 0f);
					}
				}
			}
			else if(ForwardAxis < 0)
			{
				ForwardAxis = 0;
			}
		}
	}


	public void RightMove(float Sens)
	{
		if(ShouldDo.LocalPlayerRight(Sens))
		{
			RightSens = Sens;
			if(Sens > 0)
			{
				LeftSens = 0;
				RightAxis = 1;

				if((IsOnFloor() && JumpAxis < 1) || FlyMode)
				{
					if(IsSprinting)
					{
						Momentum.x = Mathf.Clamp((float)(-1*Sens*MovementInputMultiplyer*SprintMultiplyer), -MaxMovementSpeed, 0f);
					}
					else
					{
						Momentum.x = Mathf.Clamp((float)(-1*Sens*MovementInputMultiplyer), -BaseMovementSpeed, 0f);
					}
				}
			}
			else if(RightAxis > 0)
			{
				RightAxis = 0;
			}
		}
	}


	public void LeftMove(float Sens)
	{
		if(ShouldDo.LocalPlayerLeft(Sens))
		{
			LeftSens = Sens;
			if(Sens > 0)
			{
				RightSens = 0;
				RightAxis = -1;

				if((IsOnFloor() && JumpAxis < 1) || FlyMode)
				{
					if(IsSprinting)
					{
						Momentum.x = Mathf.Clamp((float)(Sens*MovementInputMultiplyer*SprintMultiplyer), 0f, MaxMovementSpeed);
					}
					else
					{
						Momentum.x = Mathf.Clamp((float)(Sens*MovementInputMultiplyer), 0f, BaseMovementSpeed);
					}
				}
			}
			else if(RightAxis < 0)
			{
				RightAxis = 0;
			}
		}
	}


	public void Sprint(float Sens)
	{
		SprintSens = Sens;
		if(Sens > 0)
		{
			if(IsOnFloor() || FlyMode)
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

				if(FlyMode)
				{
					Momentum.y = Momentum.y*SprintMultiplyer;
				}
			}
		}
		else
		{
			if(IsOnFloor() || FlyMode)
			{
				IsSprinting = false;

				Momentum.z = Mathf.Clamp(Momentum.z, -BaseMovementSpeed, BaseMovementSpeed);
				Momentum.x = Mathf.Clamp(Momentum.x, -BaseMovementSpeed, BaseMovementSpeed);

				if(FlyMode)
				{
					Momentum.y = Mathf.Clamp(Momentum.y, -BaseMovementSpeed, BaseMovementSpeed);
				}
			}
		}
	}


	public void Jump(float Sens)
	{
		JumpSens = Sens;
		if(Sens > 0)
		{
			if(FlyMode && ShouldDo.LocalPlayerJump())
			{
				if(IsSprinting)
				{
					Momentum.y = BaseMovementSpeed*SprintMultiplyer;
				}
				else
				{
					Momentum.y = BaseMovementSpeed;
				}
				IsJumping = false;
			}
			else if(IsOnFloor() && ShouldDo.LocalPlayerJump())
			{
				Momentum.y = JumpStartForce;
				if(JumpAxis < 1)
				{
					Momentum.x *= JumpSpeedMultiplyer;
					Momentum.z *= JumpSpeedMultiplyer;
				}

				IsJumping = true;
			}

			JumpAxis = 1;
			IsCrouching = false;
		}
		else
		{
			JumpAxis = 0;
			IsJumping = false;
		}
	}


	public void Crouch(float Sens)
	{
		if(Sens > 0)
		{
			IsCrouching = true;
			JumpAxis = 0;
			JumpSens = 0;

			if(FlyMode)
			{
				if(IsSprinting)
				{
					Momentum.y = -BaseMovementSpeed*SprintMultiplyer;
				}
				else
				{
					Momentum.y = -BaseMovementSpeed;
				}
			}
		}
		else
		{
			IsCrouching = false;
		}
	}


	public void LookUp(float Sens)
	{
		if(Sens > 0)
		{
			float Change = ((float)Sens/LookDivisor)*Game.MouseSensitivity;

			if(ShouldDo.LocalPlayerPitch(Change))
			{
				LookVertical = Mathf.Clamp(LookVertical+Change, -90, 90);
				GetNode<Camera>("SteelCamera").SetRotationDegrees(new Vector3(LookVertical, 180, 0));
			}
		}
	}


	public void LookDown(float Sens)
	{
		if(Sens > 0)
		{
			float Change = ((float)Sens/LookDivisor)*Game.MouseSensitivity;

			if(ShouldDo.LocalPlayerPitch(-Change))
			{
				LookVertical = Mathf.Clamp(LookVertical-Change, -90, 90);
				GetNode<Camera>("SteelCamera").SetRotationDegrees(new Vector3(LookVertical, 180, 0));
			}
		}
	}


	public void LookRight(float Sens)
	{
		if(Sens > 0)
		{
			float Change = ((float)Sens/LookDivisor)*Game.MouseSensitivity;

			if(ShouldDo.LocalPlayerRotate(-Change))
			{
				LookHorizontal -= Change;
				SetRotationDegrees(new Vector3(0, LookHorizontal, 0));
			}
		}
	}


	public void LookLeft(float Sens)
	{
		if(Sens > 0)
		{
			float Change = ((float)Sens/LookDivisor)*Game.MouseSensitivity;

			if(ShouldDo.LocalPlayerRotate(+Change))
			{
				LookHorizontal += Change;
				SetRotationDegrees(new Vector3(0, LookHorizontal, 0));
			}
		}
	}


	public void PrimaryFire(float Sens)
	{
		if(Sens > 0 && !IsPrimaryFiring)
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
						Building.PlaceOn(Hit, GhostInstance.CurrentMeshType, 1);
						//ID 1 for now so all client own all non-default structures
					}
				}
			}
		}
		if(Sens <= 0 && IsPrimaryFiring)
		{
			IsPrimaryFiring = false;
		}
	}


	public void SecondaryFire(float Sens)
	{
		if(Sens > 0 && !IsSecondaryFiring)
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
		if(Sens <= 0 && IsSecondaryFiring)
		{
			IsSecondaryFiring = false;
		}
	}


	public void DropCurrentItem(float Sens)
	{
		if(Sens > 0)
		{
			if(Inventory[InventorySlot] != null)
			{
				Vector3 Vel = Momentum;
				if(FlyMode || IsOnFloor())
				{
					Vel = Vel.Rotated(new Vector3(0,1,0), Deg2Rad(LookHorizontal));
				}
				Vel += new Vector3(0,0,ItemThrowPower).Rotated(new Vector3(1,0,0), Deg2Rad(-LookVertical)).Rotated(new Vector3(0,1,0), Deg2Rad(LookHorizontal));

				World.DropItem(Inventory[InventorySlot], Translation+Cam.Translation, Vel);
				Inventory[InventorySlot] = null;
				HUDInstance.HotbarUpdate();
			}
		}
	}


	private void OnAir()
	{
		Momentum = Momentum.Rotated(new Vector3(0,1,0), Deg2Rad(LookHorizontal));
	}


	private void OnLand()
	{
		Momentum = Momentum.Rotated(new Vector3(0,1,0), Deg2Rad(LoopRotation(-LookHorizontal)));

		if(JumpAxis < 1)
		{
			if(ForwardAxis == 1)
			{
				if(IsSprinting)
				{
					Momentum.z = Mathf.Clamp((float)(ForwardSens*MovementInputMultiplyer*SprintMultiplyer), 0f, MaxMovementSpeed);
				}
				else
				{
					Momentum.z = Mathf.Clamp((float)(ForwardSens*MovementInputMultiplyer), 0f, BaseMovementSpeed);
				}
			}
			else if(ForwardAxis == -1)
			{
				if(IsSprinting)
				{
					Momentum.z = Mathf.Clamp((float)(-1*BackwardSens*MovementInputMultiplyer*SprintMultiplyer), -MaxMovementSpeed, 0f);
				}
				else
				{
					Momentum.z = Mathf.Clamp((float)(-1*BackwardSens*MovementInputMultiplyer), -BaseMovementSpeed, 0f);
				}
			}

			if(RightAxis == 1)
			{
				if(IsSprinting)
				{
					Momentum.x = Mathf.Clamp((float)(-1*RightSens*MovementInputMultiplyer*SprintMultiplyer), -MaxMovementSpeed, 0f);
				}
				else
				{
					Momentum.x = Mathf.Clamp((float)(-1*RightSens*MovementInputMultiplyer), -BaseMovementSpeed, 0f);
				}
			}
			else if(RightAxis == -1)
			{
				if(IsSprinting)
				{
					Momentum.x = Mathf.Clamp((float)(LeftSens*MovementInputMultiplyer*SprintMultiplyer), 0f, MaxMovementSpeed);
				}
				else
				{
					Momentum.x = Mathf.Clamp((float)(LeftSens*MovementInputMultiplyer), 0f, BaseMovementSpeed);
				}
			}

			if(SprintSens > 0 && !IsSprinting)
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
			else if(SprintSens <= 0 && IsSprinting)
			{
				IsSprinting = false;

				Momentum.z = Mathf.Clamp(Momentum.z, -BaseMovementSpeed, BaseMovementSpeed);
				Momentum.x = Mathf.Clamp(Momentum.x, -BaseMovementSpeed, BaseMovementSpeed);
			}
		}
	}


	public override void _PhysicsProcess(float Delta)
	{
		if(!Possessed || Frozen)
		{
			return;
		}


		if(!FlyMode)
		{
			if(IsOnFloor())
			{
				if(!WasOnFloor)
				{
					OnLand();
				}
			}
			else if(WasOnFloor)
			{
				OnAir();
			}
			WasOnFloor = IsOnFloor();
		}

		if(JumpAxis < 1)
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
		}
		else if(!FlyMode)
		{
			Jump(JumpSens);
		}

		if(FlyMode)
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

			if(JumpAxis < 1 && !IsCrouching)
			{
				if(Momentum.y > 0)
				{
					Momentum.y = Mathf.Clamp(Momentum.y-Friction*Delta, 0f, MaxMovementSpeed);
				}
				if(Momentum.y < 0)
				{
					Momentum.y = Mathf.Clamp(Momentum.y+Friction*Delta, 0f, MaxMovementSpeed);
				}
			}
		}

		if(!FlyMode)
		{
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

			if(!IsOnFloor())
			{
				Momentum = AirAccelerate(Momentum, new Vector3(-RightAxis*MovementInputMultiplyer, 0, ForwardAxis*MovementInputMultiplyer).Rotated(new Vector3(0,1,0), Deg2Rad(LookHorizontal)), Delta);
			}
		}

		Vector3 OldPos = Translation;
		//100 bounces in order to allow players to go up slopes more quickly
		//MoveAndSlide multiplies by *physics* delta internally
		if(FlyMode)
		{
			Vector3 FlatVel = Momentum;
			FlatVel.y = 0;
			MoveAndSlide(FlatVel.Rotated(new Vector3(1,0,0), Mathf.Deg2Rad(LoopRotation(-LookVertical)))
			             .Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(LookHorizontal)), new Vector3(0,1,0), true, 100, Mathf.Deg2Rad(60));

			MoveAndSlide(new Vector3(0,Momentum.y,0).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(LookHorizontal)), new Vector3(0,1,0), true, 100, Mathf.Deg2Rad(60));
		}
		else if(IsOnFloor())
		{
			MoveAndSlide(Momentum.Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(LookHorizontal)), new Vector3(0,1,0), true, 100, Mathf.Deg2Rad(60));
		}
		else
		{
			Momentum = MoveAndSlide(Momentum, new Vector3(0,1,0), true, 100, Mathf.Deg2Rad(60));
		}
		Vector3 NewPos = Translation;
		Translation = OldPos;
		if(NewPos != OldPos)
		{
			if(ShouldDo.LocalPlayerMove(NewPos))
			{
				Translation = NewPos;
			}
		}

		if(!FlyMode && IsOnFloor() && Momentum.y <= 0f)
		{
			Momentum.y = -1f;
		}

		Net.SteelRpcUnreliable(this, nameof(Update), Translation, RotationDegrees);

		if(!Building.GetChunkTuple(Translation).Equals(CurrentChunk))
		{
			CurrentChunk = Building.GetChunkTuple(Translation);
			Net.UnloadAndRequestChunks();
		}
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
