using Godot;
using static Godot.Mathf;
using static SteelMath;
using System;
using System.Collections.Generic;


public class Player : KinematicBody
{
	public bool Possessed = false;
	public int Id = 0;

	private const float BaseMovementSpeed = 20;
	private const float SprintMultiplyer = 2; //Speed while sprinting is base speed times this value
	private const float MaxMovementSpeed = BaseMovementSpeed*SprintMultiplyer;
	private const float AirAcceleration = 24; //How many units per second to accelerate
	private const float Friction = MaxMovementSpeed / 0.2f; //The number is how many seconds needed to stop from full speed
	private const float JumpSpeedMultiplyer = 1.2f;
	private const float JumpStartForce = 8f;
	private const float JumpContinueForce = 6f;
	private const float MaxJumpLength = 0.3f;
	private const float WallKickJumpForce = 16;
	private const float WallKickHorzontalForce = 45;
	private const float MinWallKickRecoverPercentage = 0.2f;
	private const float WallKickRecoverSpeed= 100 / 25; //Latter number percent of a second it takes to fully recover
	private const float Gravity = 14f;
	private const float ItemThrowPower = 15f;
	private const float ItemPickupDistance = 8f;
	private const float MinItemPickupLife = 1; //In seconds
	private const float LookDivisor = 6;

	private const float SfxMinLandMomentumY = 3;

	private bool Frozen = true;
	public bool FlyMode { get; private set;} = false;

	public System.Tuple<int, int> CurrentChunk = new System.Tuple<int, int>(0, 0);

	private int ForwardAxis = 0;
	private int RightAxis = 0;
	private int JumpAxis = 0;

	public float ForwardSens = 0;
	public float BackwardSens = 0;
	public float RightSens = 0;
	public float LeftSens = 0;
	public float SprintSens = 0;
	public float JumpSens = 0;

	public bool IsCrouching = false;
	public bool IsSprinting = false;
	public bool IsJumping = false;
	public bool WasOnFloor = false;
	private float JumpTimer = 0f;
	private float WallKickRecoverPercentage = 1;
	private Vector3 Momentum = new Vector3(0,0,0);
	private float LastMomentumY = 0;
	private float LookHorizontal = 0;
	private float LookVertical = 0;
	private bool IsPrimaryFiring = false;
	private bool IsSecondaryFiring = false;

	public Items.Instance[] Inventory = new Items.Instance[10];
	public int InventorySlot = 0;

	public int BuildRotation = 0;

	public Camera Cam;

	public HUD HUDInstance;
	private Ghost GhostInstance;

	public PlayerSfxManager SfxManager;

	Player()
	{
		if(Engine.EditorHint) {return;}

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

			GhostInstance = ((PackedScene)(GD.Load("res://World/Ghost.tscn"))).Instance() as Ghost;
			GhostInstance.Hide();
			GetParent().CallDeferred("add_child", GhostInstance);

			SfxManager = GetNode<PlayerSfxManager>("PlayerSfxManager");
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

		ItemGive(new Items.Instance(Items.TYPE.PLATFORM));
		ItemGive(new Items.Instance(Items.TYPE.WALL));
		ItemGive(new Items.Instance(Items.TYPE.SLOPE));
	}


	public Vector3 CenterPosition()
	{
		return new Vector3(0, 3.4f, 0) + Translation;
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


	public void ToggleFly()
	{
		if(Game.Mode.ShouldToggleFly())
			SetFly(!FlyMode);
	}


	public void PositionReset()
	{
		Translation = new Vector3(0,1,0);
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
					HUDInstance.HotbarUpdate();
					return;
				}
			}
		}

		for(int Slot = 0; Slot <= 9; Slot++)
		{
			if(Inventory[Slot] is null)
			{
				Inventory[Slot] = ToGive;
				HUDInstance.HotbarUpdate();
				return;
			}
		}
	}


	[Remote]
	public void PickupItem(Items.TYPE Type)
	{
		ItemGive(new Items.Instance(Type));
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
		if(Game.Mode.ShouldMoveForward(Sens))
		{
			ForwardSens = Sens;
			if(Sens > 0)
			{
				ForwardAxis = 1;
			}
			else if(ForwardAxis > 0)
			{
				ForwardAxis = 0;
				if(BackwardSens > 0)
				{
					ForwardAxis = -1;
				}
			}
		}
	}


	public void BackwardMove(float Sens)
	{
		if(Game.Mode.ShouldMoveBackward(Sens))
		{
			BackwardSens = Sens;
			if(Sens > 0)
			{
				ForwardAxis = -1;
			}
			else if(ForwardAxis < 0)
			{
				ForwardAxis = 0;
				if(ForwardSens > 0)
				{
					ForwardAxis = 1;
				}
			}
		}
	}


	public void RightMove(float Sens)
	{
		if(Game.Mode.ShouldMoveRight(Sens))
		{
			RightSens = Sens;
			if(Sens > 0)
			{
				RightAxis = 1;
			}
			else if(RightAxis > 0)
			{
				RightAxis = 0;
				if(LeftSens > 0)
				{
					RightAxis = -1;
				}
			}
		}
	}


	public void LeftMove(float Sens)
	{
		if(Game.Mode.ShouldMoveLeft(Sens))
		{
			LeftSens = Sens;
			if(Sens > 0)
			{
				RightAxis = -1;
			}
			else if(RightAxis < 0)
			{
				RightAxis = 0;
				if(RightSens >0)
				{
					RightAxis = 1;
				}
			}
		}
	}


	public void Sprint(float Sens)
	{
		SprintSens = Sens;
		if(Sens > 0)
		{
			IsSprinting = true;
		}
		else
		{
			IsSprinting = false;
		}
	}


	public void Jump(float Sens)
	{
		JumpSens = Sens;
		if(Sens > 0)
		{
			if(FlyMode)
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
			else if(WallKickRecoverPercentage >= MinWallKickRecoverPercentage && IsOnFloor() && Game.Mode.ShouldJump())
			{
				Momentum.y = JumpStartForce;
				if(JumpAxis < 1)
				{
					Vector3 FlatMomentum = new Vector3(Momentum.x, 0, Momentum.z);
					FlatMomentum = FlatMomentum.Normalized() * (FlatMomentum.Length() + JumpSpeedMultiplyer);
					Momentum.x = FlatMomentum.x;
					Momentum.z = FlatMomentum.z;
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

			if(FlyMode && Game.Mode.ShouldCrouch()) //NOTE Crouching is currently only for going down in flymode
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
			float Change = ((float)Sens/LookDivisor)*Game.LookSensitivity;

			if(Game.Mode.ShouldPlayerPitch(Change))
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
			float Change = ((float)Sens/LookDivisor)*Game.LookSensitivity;

			if(Game.Mode.ShouldPlayerPitch(-Change))
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
			float Change = ((float)Sens/LookDivisor)*Game.LookSensitivity;

			if(Game.Mode.ShouldPlayerRotate(-Change))
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
			float Change = ((float)Sens/LookDivisor)*Game.LookSensitivity;

			if(Game.Mode.ShouldPlayerRotate(+Change))
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
						World.PlaceOn(Hit, GhostInstance.CurrentMeshType, 1);
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
					Hit.NetRemove();
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
				Vel += new Vector3(0,0,ItemThrowPower)
					.Rotated(new Vector3(1,0,0), Deg2Rad(-LookVertical))
					.Rotated(new Vector3(0,1,0), Deg2Rad(LookHorizontal));

				World.Self.DropItem(Inventory[InventorySlot].Type, Translation+Cam.Translation, Vel);

				if(Inventory[InventorySlot].Count > 1)
					Inventory[InventorySlot].Count -= 1;
				else
					Inventory[InventorySlot] = null;
				HUDInstance.HotbarUpdate();
			}
		}
	}


	private Vector3 AirAccelerate(Vector3 Vel, Vector3 WishDir, float Delta)
	{
		WishDir = ClampVec3(WishDir, 0, 1) * ((MaxMovementSpeed + BaseMovementSpeed) / 2);
		float CurrentSpeed = Vel.Dot(WishDir);
		float AddSpeed = MaxMovementSpeed - CurrentSpeed;
		AddSpeed = Clamp(AddSpeed, 0, AirAcceleration*Delta);
		return Vel + WishDir * AddSpeed;
	}


	public override void _PhysicsProcess(float Delta)
	{
		if(!Possessed || Frozen)
		{
			return;
		}

		{
			List<DroppedItem> ToPickUpList = new List<DroppedItem>();
			foreach(DroppedItem Item in World.ItemList)
			{
				if(CenterPosition().DistanceTo(Item.Translation) <= ItemPickupDistance && Item.Life >= MinItemPickupLife)
				{
					ToPickUpList.Add(Item);
				}
			}
			foreach(DroppedItem Item in ToPickUpList)
			{
				World.Self.RequestDroppedItem(Net.Work.GetNetworkUniqueId(), Item.GetName());
				World.ItemList.Remove(Item);
				Item.Remove();
			}
		}

		WallKickRecoverPercentage = Clamp(WallKickRecoverPercentage + Delta*WallKickRecoverSpeed, 0, 1);

		if(JumpAxis > 0 && WallKickRecoverPercentage >= MinWallKickRecoverPercentage && IsOnFloor())
		{
			Momentum.y = JumpStartForce;
			IsJumping = true;
		}

		if(IsJumping && !WasOnFloor)
		{
			Momentum.y += JumpContinueForce*Delta;

			JumpTimer += Delta;
			if(JumpTimer >= MaxJumpLength)
			{
				JumpTimer = 0;
				IsJumping = false;
			}
		}

		if(!IsJumping && !FlyMode)
		{
			Momentum.y = Mathf.Clamp(Momentum.y - Gravity*Delta, -MaxMovementSpeed, MaxMovementSpeed);
		}

		if(FlyMode && JumpAxis <= 0 && !IsCrouching)
		{
			//In flymode and jump is not being held
			if(Momentum.y > 0)
			{
				Momentum.y = Mathf.Clamp(Momentum.y - Friction*Delta, 0, MaxMovementSpeed);
			}
			else if(Momentum.y < 0)
			{
				Momentum.y = Mathf.Clamp(Momentum.y + Friction*Delta, -MaxMovementSpeed, 0);
			}
		}

		if(IsOnFloor() && !WasOnFloor && Abs(LastMomentumY) > SfxMinLandMomentumY)
		{
			float Volume = Abs(Clamp(LastMomentumY, -MaxMovementSpeed, 0))/2 - 30;
			SfxManager.FpLand(Volume);
		}

		WasOnFloor = IsOnFloor();

		if(!IsJumping && (IsOnFloor() || FlyMode))
		{
			float SpeedLimit = BaseMovementSpeed;
			if(IsSprinting)
			{
				SpeedLimit *= SprintMultiplyer;
			}

			float X = 0, Z = 0;
			if(RightAxis > 0)
				X = -RightSens;
			else if(RightAxis < 0)
				X = LeftSens;
			if(ForwardAxis > 0)
				Z = ForwardSens;
			else if(ForwardAxis < 0)
				Z = -BackwardSens;

			Vector3 WishDir = ClampVec3(new Vector3(X, 0, Z), 0, 1) * (SpeedLimit + Friction*Delta);
			WishDir = WishDir.Rotated(new Vector3(0,1,0), Deg2Rad(LookHorizontal));
			if(WishDir.Length() > 0)
			{
				Momentum.x = WishDir.x;
				Momentum.z = WishDir.z;
			}

			float Speed = Momentum.Length();
			if(Speed > 0)
			{
				Speed = Clamp(Speed - Friction*Delta, 0, Speed);
				Vector3 HorzMomentum = new Vector3(Momentum.x, 0, Momentum.z).Normalized() * Speed;
				Momentum.x = HorzMomentum.x;
				Momentum.z = HorzMomentum.z;
			}
		}
		else
		{
			float X = 0, Z = 0;
			if(RightAxis > 0)
				X = -RightSens;
			else if(RightAxis < 0)
				X = LeftSens;
			if(ForwardAxis > 0)
				Z = ForwardSens;
			else if(ForwardAxis < 0)
				Z = -BackwardSens;

			Vector3 WishDir = new Vector3(X, 0, Z);
			WishDir = WishDir.Rotated(new Vector3(0,1,0), Deg2Rad(LookHorizontal)) * WallKickRecoverPercentage;
			Momentum = AirAccelerate(Momentum, WishDir, Delta);
		}

		LastMomentumY = Momentum.y;

		Vector3 OldPos = Translation;
		if(FlyMode)
		{
			Vector3 FlatVel = Momentum;
			FlatVel.y = 0;
			MoveAndSlide(FlatVel
			             .Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(LoopRotation(-LookHorizontal)))
			             .Rotated(new Vector3(1,0,0), Mathf.Deg2Rad(LoopRotation(-LookVertical)))
			             .Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(LoopRotation(LookHorizontal))),
			             new Vector3(0,1,0), true, 100, Mathf.Deg2Rad(60));

			MoveAndSlide(new Vector3(0,Momentum.y,0).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(LookHorizontal)), new Vector3(0,1,0), true, 100, Mathf.Deg2Rad(60))
				.Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(LoopRotation(-LookHorizontal)));
		}
		else
		{
			Momentum = MoveAndSlide(Momentum, new Vector3(0,1,0), true, 100, Mathf.Deg2Rad(60));

			if(JumpAxis > 0 && WallKickRecoverPercentage >= MinWallKickRecoverPercentage && IsOnWall() && GetSlideCount() > 0)
			{
				WallKickRecoverPercentage = 0;

				Momentum += WallKickHorzontalForce * GetSlideCollision(0).Normal;
				Momentum.y = WallKickJumpForce;

				SfxManager.FpWallKick();
			}
		}
		Vector3 NewPos = Translation;
		Translation = OldPos;
		if(NewPos != OldPos)
		{
			if(Game.Mode.ShouldPlayerMove(NewPos))
			{
				Translation = NewPos;
			}
		}

		if(!FlyMode && IsOnFloor() && Momentum.y <= 0f)
		{
			Momentum.y = -1f;
		}

		Net.SteelRpcUnreliable(this, nameof(Update), Translation, RotationDegrees);

		if(!World.GetChunkTuple(Translation).Equals(CurrentChunk))
		{
			CurrentChunk = World.GetChunkTuple(Translation);
			Net.UnloadAndRequestChunks();
		}
	}


	[Remote]
	public void Update(Vector3 Position, Vector3 Rotation)
	{
		if(Game.Mode.ShouldSyncRemotePlayerPosition(Id, Position))
		{
			Translation = Position;
		}

		if(Game.Mode.ShouldSyncRemotePlayerRotation(Id, Rotation))
		{
			RotationDegrees = Rotation;
		}
	}
}
