using Godot;
using static Godot.Mathf;
using static SteelMath;
using System;
using System.Collections.Generic;
using static System.Diagnostics.Debug;


public class Player : KinematicBody, IPushable
{
	public bool Possessed = false;
	public int Id = 0;

	public float BaseMovementSpeed = 20;
	public float SprintMultiplyer = 2; //Speed while sprinting is base speed times this value
	public float FlySprintMultiplyer = 6; //Speed while sprint flying is base speed times this value
	public float MaxMovementSpeed { get { return BaseMovementSpeed*SprintMultiplyer; } }
	public float CrouchMovementDivisor = 1.5f;
	public float MaxVerticalSpeed = 100f;
	public float AirAcceleration = 22; //How many units per second to accelerate
	public float DecelerateTime = 0.15f; //How many seconds needed to stop from full speed
	public float Friction { get { return MaxMovementSpeed / DecelerateTime; } }
	public float SlideFrictionDivisor = 10;
	public float FlyDecelerateTime = 0.15f; //How many seconds needed to stop from full speed
	public float FlyFriction { get { return (BaseMovementSpeed*FlySprintMultiplyer) / FlyDecelerateTime; } }
	public float CrouchDownForce = -50f;
	public float JumpSpeedAddend = 18f;
	public float JumpStartForce = 12f;
	public float JumpContinueForce = 5f;
	public float MaxJumpLength = 0.3f;
	public float WallKickJumpForce = 22;
	public float WallKickHorzontalForce = 35;
	public float Gravity = 25f;
	public float ItemThrowPower = 20f;
	public float ItemPickupDistance = 8f;
	public float SlotSwitchCooldown = 15;
	public float BuildingCooldown = 15;
	public float MaxGroundLegRotation = 50;
	public float MaxAirLegRotation = 80;
	public float MaxHealth = 100;
	public float LookDivisor = 6;

	public static float MinAdsMultiplyer = 0.7f;
	public static float AdsTime = 0.15f; //Seconds to achieve full ads

	private const float SfxMinLandMomentumY = 3;

	public bool Ads = false;
	public float AdsMultiplyer = 1;

	private bool Frozen = true;
	public bool FlyMode { get; private set;} = false;

	public System.Tuple<int, int> CurrentChunk = new System.Tuple<int, int>(0, 0);

	public float Health = 0;
	private int __team = 1;
	public int Team
	{
		get { return __team; }
		set
		{
			__team = value;

			if(Net.Work.GetNetworkUniqueId() == Id) //Just in case
				Net.SteelRpc(this, nameof(NotifyTeamChange), value);
		}
	}

	public int ForwardAxis = 0;
	public int RightAxis = 0;
	public int JumpAxis = 0;

	public float ForwardSens = 0;
	public float BackwardSens = 0;
	public float RightSens = 0;
	public float LeftSens = 0;
	public float SprintSens = 0;
	public float JumpSens = 0;

	public bool IsCrouching = false;
	public bool IsSprinting = false;
	public bool IsJumping = false;
	public bool HasJumped = false;
	public bool WasOnFloor = false;
	public float JumpTimer = 0f;
	public Vector3 Momentum = new Vector3(0,0,0);
	public float LastMomentumY = 0;
	public float LookHorizontal = 0;
	public float IntendedLookVertical = 0; //Mouse input affects this
	public float ActualLookVertical = 0; //Intended + additive recoil offset
	public bool IsPrimaryFiring = false;
	public bool IsSecondaryFiring = false;

	//For these *please* use SetCooldown
	public float CurrentMaxCooldown { get; private set;} = 100;
	public float CurrentCooldown { get; private set;} = 100;
	public bool PreventSwitch { get; private set;} = false;

	public List<Hitscan.AdditiveRecoil> ActiveAdditiveRecoil = new List<Hitscan.AdditiveRecoil>();

	public Items.Instance[] Inventory = new Items.Instance[10];
	public int InventorySlot = 0;

	public int BuildRotation = 0;

	public Camera Cam;
	public MeshInstance ViewmodelItem;
	public Spatial ProjectileEmitterHinge;
	public Spatial ProjectileEmitter;

	public CollisionShape CollisionCapsule;

	public Spatial HeadJoint;
	public Spatial LegsJoint;
	public MeshInstance ThirdPersonItem;

	public MeshInstance LowerLegs;

	public CPUParticles RightLegFlames;
	public CPUParticles LeftLegFlames;

	public HUD HUDInstance;
	public Ghost GhostInstance;

	public PlayerSfxManager SfxManager;

	Player()
	{
		if(Engine.EditorHint) {return;}

		HUDInstance = ((PackedScene)GD.Load("res://UI/HUD.tscn")).Instance() as HUD;
	}


	public override void _Ready()
	{
		Cam = GetNode<Camera>("SteelCamera");

		ViewmodelItem = GetNode<MeshInstance>("SteelCamera/ViewmodelArm/ViewmodelItem");
		ViewmodelItem.Hide();

		ProjectileEmitterHinge = GetNode<Spatial>("ProjectileEmitterHinge");
		ProjectileEmitter = GetNode<Spatial>("ProjectileEmitterHinge/ProjectileEmitter");

		CollisionCapsule = GetNode<CollisionShape>("CollisionShape");

		if(Possessed)
		{
			Cam.MakeCurrent();
			GetNode<RayCast>("SteelCamera/RayCast").AddException(this);
			GetNode<Spatial>("BodyScene").Free();

			AddChild(HUDInstance);

			GhostInstance = ((PackedScene)(GD.Load("res://World/Ghost.tscn"))).Instance() as Ghost;
			GhostInstance.Hide();
			GetParent().CallDeferred("add_child", GhostInstance);

			SfxManager = GetNode<PlayerSfxManager>("PlayerSfxManager");
		}
		else
		{
			HeadJoint = GetNode("BodyScene").GetNode<Spatial>("HeadJoint");
			LegsJoint = GetNode("BodyScene").GetNode<Spatial>("LegsJoint");

			LowerLegs = GetNode("BodyScene").GetNode<MeshInstance>("LegsJoint/LowerLegs");

			RightLegFlames = GetNode("BodyScene").GetNode<CPUParticles>("LegsJoint/LowerLegs/LegFlames/Right");
			LeftLegFlames = GetNode("BodyScene").GetNode<CPUParticles>("LegsJoint/LowerLegs/LegFlames/Left");

			ThirdPersonItem = GetNode("BodyScene").GetNode<MeshInstance>("ItemMesh");
			ShaderMaterial Mat = new ShaderMaterial();
			Mat.Shader = Items.TileShader;
			ThirdPersonItem.MaterialOverride = Mat;

			GetNode<MeshInstance>("SteelCamera/ViewmodelArm").Hide();
			GetNode<CPUParticles>("SteelCamera/ViewmodelArm/Forcefield").Hide();

			Spatial Body = GetNode<Spatial>("BodyScene");
			Body.GetNode<HitboxClass>("BodyHitbox").OwningPlayer = this;
			Body.GetNode<HitboxClass>("HeadJoint/HeadHitbox").OwningPlayer = this;
			Body.GetNode<HitboxClass>("LegsJoint/LegsHitbox").OwningPlayer = this;

			SetProcess(false);

			return;
		}

		Respawn();
		if(GetTree().IsNetworkServer())
			SetFreeze(false);

		ItemGive(new Items.Instance(Items.ID.PLATFORM));
		ItemGive(new Items.Instance(Items.ID.WALL));
		ItemGive(new Items.Instance(Items.ID.SLOPE));
		ItemGive(new Items.Instance(Items.ID.TRIANGLE_WALL));
		ItemGive(new Items.Instance(Items.ID.ROCKET_JUMPER));
		ItemGive(new Items.Instance(Items.ID.THUNDERBOLT));
		ItemGive(new Items.Instance(Items.ID.SCATTERSHOCK));
		ItemGive(new Items.Instance(Items.ID.SWIFTSPARK));
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


	public void ApplyPush(Vector3 Push)
	{
		Momentum += Push;
	}


	public void MovementReset()
	{
		if(Game.Mode.ShouldMovementReset())
		{
			Translation = new Vector3(0, 5.1f + 0.15f, 0);
			Momentum = new Vector3();
		}
	}


	public void Respawn()
	{
		HUDInstance.ClearDamageIndicators();
		MovementReset();
		Ads = false;
		Health = MaxHealth;
	}


	[Remote]
	public void ApplyDamage(float Damage, Vector3 Origin)
	{
		Health = Clamp(Health - Damage, 0, MaxHealth);
		HUDInstance.AddDamageIndicator(Origin, Damage);
	}


	public void ItemGive(Items.Instance ToGive)
	{
		for(int Slot = 0; Slot <= 9; Slot++)
		{
			if(!(Inventory[Slot] is null)) //If inventory item is not null
			{
				if(Inventory[Slot].Id == ToGive.Id)
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


	public void SetCooldown(float NewCooldown, float NewMaxCooldown, bool NewPreventSwitch)
	{
		CurrentCooldown = Clamp(NewCooldown, 0, NewMaxCooldown);
		CurrentMaxCooldown = NewMaxCooldown;
		PreventSwitch = NewPreventSwitch;
	}


	[Remote]
	public void PickupItem(Items.ID Type)
	{
		ItemGive(new Items.Instance(Type));
	}


	public void InventoryUp()
	{
		if(!(CurrentCooldown < CurrentMaxCooldown && PreventSwitch))
		{
			BuildRotation = 0;

			InventorySlot--;
			if(InventorySlot < 0)
			{
				InventorySlot = 9;
			}

			HUDInstance.HotbarUpdate();
			Hitscan.Reset();
			SetCooldown(0, SlotSwitchCooldown, false);
			Ads = false;
		}
	}


	public void InventoryDown()
	{
		if(!(CurrentCooldown < CurrentMaxCooldown && PreventSwitch))
		{
			BuildRotation = 0;

			InventorySlot++;
			if(InventorySlot > 9)
			{
				InventorySlot = 0;
			}

			HUDInstance.HotbarUpdate();
			Hitscan.Reset();
			SetCooldown(0, SlotSwitchCooldown, false);
			Ads = false;
		}
	}


	public void InventorySlotSelect(int Slot)
	{
		if(!(CurrentCooldown < CurrentMaxCooldown && PreventSwitch))
		{
			InventorySlot = Slot;
			HUDInstance.HotbarUpdate();

			Hitscan.Reset();
			SetCooldown(0, SlotSwitchCooldown, false);
			Ads = false;
		}
	}


	public void InventorySlot0() { InventorySlotSelect(0); }
	public void InventorySlot1() { InventorySlotSelect(1); }
	public void InventorySlot2() { InventorySlotSelect(2); }
	public void InventorySlot3() { InventorySlotSelect(3); }
	public void InventorySlot4() { InventorySlotSelect(4); }
	public void InventorySlot5() { InventorySlotSelect(5); }
	public void InventorySlot6() { InventorySlotSelect(6); }
	public void InventorySlot7() { InventorySlotSelect(7); }
	public void InventorySlot8() { InventorySlotSelect(8); }
	public void InventorySlot9() { InventorySlotSelect(9); }


	public void BuildRotate(float Sens)
	{
		if(Sens > 0 && Inventory[InventorySlot] != null)
		{
			BuildRotation++;
			if(BuildRotation > 3)
				BuildRotation = 0;
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
		if(Sens > 0 && !IsCrouching && !Ads)
		{
			IsSprinting = true;

			if(FlyMode)
			{
				if(JumpAxis == 1)
					Momentum.y = BaseMovementSpeed*FlySprintMultiplyer;
				else if(IsCrouching)
					Momentum.y = -BaseMovementSpeed*FlySprintMultiplyer;
			}
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
					Momentum.y = BaseMovementSpeed*FlySprintMultiplyer;
				}
				else
				{
					Momentum.y = BaseMovementSpeed;
				}
				IsJumping = false;
			}
			else if(IsOnFloor() && Game.Mode.ShouldJump())
			{
				Momentum.y = JumpStartForce;
				if(JumpAxis < 1)
				{
					Vector3 FlatMomentum = new Vector3(Momentum.x, 0, Momentum.z);
					FlatMomentum = FlatMomentum.Normalized() * (FlatMomentum.Length() + JumpSpeedAddend);
					Momentum.x = FlatMomentum.x;
					Momentum.z = FlatMomentum.z;
				}

				IsJumping = true;
				HasJumped = true;
			}

			JumpAxis = 1;
		}
		else
		{
			JumpAxis = 0;
			IsJumping = false;
			HasJumped = false;
		}
	}


	public void Crouch(float Sens)
	{
		if(Sens > 0)
		{
			IsCrouching = true;

			if(!FlyMode)
				IsSprinting = false;

			if(Game.Mode.ShouldCrouch())
			{
				if(FlyMode)
				{
					JumpAxis = 0;
					JumpSens = 0;

					if(IsSprinting)
						Momentum.y = -BaseMovementSpeed*FlySprintMultiplyer;
					else
						Momentum.y = -BaseMovementSpeed;
				}
				else if(!IsOnFloor())
				{
					IsJumping = false;
					if(Momentum.y > CrouchDownForce)
						Momentum.y = CrouchDownForce;
				}
			}

			CollisionCapsule.Scale = new Vector3(1.5f, 1.5f, 1);
			CollisionCapsule.Translation = new Vector3(0, 1.5f, 0);
			if(IsOnFloor())
				Translation = new Vector3(Translation.x, Translation.y-1.5f, Translation.z);
		}
		else
		{
			IsCrouching = false;

			if(SprintSens > 0)
				Sprint(SprintSens);

			CollisionCapsule.Scale = new Vector3(1.5f, 1.5f, 1.5f);
			CollisionCapsule.Translation = new Vector3(0, 0, 0);
			if(IsOnFloor())
				Translation = new Vector3(Translation.x, Translation.y+1.5f, Translation.z);
		}
	}


	public void ApplyLookVertical(float Change)
	{
		IntendedLookVertical = Mathf.Clamp(IntendedLookVertical+Change, -90, 90);

		ActualLookVertical = IntendedLookVertical;
		foreach(Hitscan.AdditiveRecoil Instance in ActiveAdditiveRecoil)
		{
			ActualLookVertical = Clamp(ActualLookVertical + Instance.CaclulateOffset(), -90, 90);
		}
		Cam.SetRotationDegrees(new Vector3(ActualLookVertical, 180, 0));
		ProjectileEmitterHinge.SetRotationDegrees(new Vector3(ActualLookVertical, 180, 0));
	}


	public void LookUp(float Sens)
	{
		if(Sens > 0)
		{
			float Change = ((float)Sens/LookDivisor)*Game.LookSensitivity*AdsMultiplyer;

			if(Game.Mode.ShouldPlayerPitch(Change))
				ApplyLookVertical(Change);
		}
	}


	public void LookDown(float Sens)
	{
		if(Sens > 0)
		{
			float Change = ((float)Sens/LookDivisor)*Game.LookSensitivity*AdsMultiplyer;

			if(Game.Mode.ShouldPlayerPitch(-Change))
				ApplyLookVertical(-Change);
		}
	}


	public void LookRight(float Sens)
	{
		if(Sens > 0)
		{
			float Change = ((float)Sens/LookDivisor)*Game.LookSensitivity*AdsMultiplyer;

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
			float Change = ((float)Sens/LookDivisor)*Game.LookSensitivity*AdsMultiplyer;

			if(Game.Mode.ShouldPlayerRotate(+Change))
			{
				LookHorizontal += Change;
				SetRotationDegrees(new Vector3(0, LookHorizontal, 0));
			}
		}
	}


	public void PrimaryFire(float Sens)
	{
		if(Sens > 0 && !IsPrimaryFiring && CurrentCooldown >= CurrentMaxCooldown)
		{
			IsPrimaryFiring = true;

			if(Inventory[InventorySlot] != null)
			{
				if(Items.IdInfos[Inventory[InventorySlot].Id].PositionDelegate != null)
				{
					RayCast BuildRayCast = GetNode("SteelCamera/RayCast") as RayCast;
					if(BuildRayCast.IsColliding())
					{
						Tile Base = BuildRayCast.GetCollider() as Tile;
						if(Base != null && GhostInstance.CanBuild)
						{
							Vector3? PlacePosition = Items.TryCalculateBuildPosition(GhostInstance.CurrentMeshType, Base, RotationDegrees.y, BuildRotation, BuildRayCast.GetCollisionPoint());
							if(PlacePosition != null
							   && Game.Mode.ShouldPlaceTile(GhostInstance.CurrentMeshType,
							                                PlacePosition.Value,
							                                Items.CalculateBuildRotation(GhostInstance.CurrentMeshType,
							                                                             Base, RotationDegrees.y, BuildRotation,
							                                                             BuildRayCast.GetCollisionPoint())))
							{
								World.PlaceOn(GhostInstance.CurrentMeshType, Base, RotationDegrees.y, BuildRotation, BuildRayCast.GetCollisionPoint(), 1); //ID 1 for now so all client own all non-default structures
								SetCooldown(0, BuildingCooldown, true);
							}
						}
					}
				}

				if(Items.IdInfos[Inventory[InventorySlot].Id].UseDelegate != null)
				{
					Items.UseItem(Inventory[InventorySlot], this);
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

			Items.Instance CurrentItem = Inventory[InventorySlot];

			if(CurrentItem == null || !Items.IdInfos[CurrentItem.Id].CanAds)
			{
				if(CurrentCooldown >= CurrentMaxCooldown)
				{
					RayCast BuildRayCast = GetNode("SteelCamera/RayCast") as RayCast;
					if(BuildRayCast.IsColliding())
					{
						Tile Hit = BuildRayCast.GetCollider() as Tile;
						if(Hit != null && Game.Mode.ShouldRemoveTile(Hit.Type, Hit.Translation, Hit.RotationDegrees, Hit.OwnerId))
						{
							Hit.NetRemove();
							SetCooldown(0, BuildingCooldown, true);
						}
					}
				}
			}

			else if(CurrentItem != null && Items.IdInfos[CurrentItem.Id].CanAds)
			{
				Ads = true;
				IsSprinting = false;
			}
		}

		if(Sens <= 0 && IsSecondaryFiring)
		{
			IsSecondaryFiring = false;

			Items.Instance CurrentItem = Inventory[InventorySlot];
			if(CurrentItem != null && Items.IdInfos[CurrentItem.Id].CanAds)
			{
				Ads = false;
				if(SprintSens > 0)
					Sprint(SprintSens);
			}
		}
	}


	public void ThrowCurrentItem(float Sens)
	{
		if(Sens > 0)
		{
			if(Inventory[InventorySlot] != null && Game.Mode.ShouldThrowItem())
			{
				Vector3 Vel = Momentum + new Vector3(0,0,ItemThrowPower)
					.Rotated(new Vector3(1,0,0), Deg2Rad(-ActualLookVertical))
					.Rotated(new Vector3(0,1,0), Deg2Rad(LookHorizontal));

				World.Self.DropItem(Inventory[InventorySlot].Id, Translation+Cam.Translation, Vel);

				if(Inventory[InventorySlot].Count > 1)
					Inventory[InventorySlot].Count -= 1;
				else
					Inventory[InventorySlot] = null;
				HUDInstance.HotbarUpdate();

				SfxManager.FpThrow();
				SetCooldown(0, SlotSwitchCooldown, false);
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
			return;

		if(Health <= 0)
			Respawn();

		{
			List<DroppedItem> ToPickUpList = new List<DroppedItem>();
			foreach(DroppedItem Item in World.ItemList)
			{
				if(Translation.DistanceTo(Item.Translation) <= ItemPickupDistance && Item.Life >= DroppedItem.MinPickupLife)
				{
					PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
					Godot.Collections.Dictionary Results = State.IntersectRay(Translation, Item.Translation, new Godot.Collections.Array{this}, 2);
					if(Results.Count <= 0)
						ToPickUpList.Add(Item);
				}
			}
			if(ToPickUpList.Count > 0)
			{
				SfxManager.FpPickup();
				SetCooldown(0, SlotSwitchCooldown, false);

				foreach(DroppedItem Item in ToPickUpList)
				{
					if(Game.Mode.ShouldPickupItem(Item.Type))
					{
						World.Self.RequestDroppedItem(Net.Work.GetNetworkUniqueId(), Item.GetName());
						World.ItemList.Remove(Item);
					}
				}
			}
		}

		CurrentCooldown = Clamp(CurrentCooldown + (100*Delta), 0, CurrentMaxCooldown);

		if(JumpAxis > 0 && IsOnFloor() && !Ads)
		{
			Momentum.y = JumpStartForce;
			IsJumping = true;
			HasJumped = true;
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
			Momentum.y = Mathf.Clamp(Momentum.y - Gravity*Delta, -MaxVerticalSpeed, MaxVerticalSpeed);
		}

		if(FlyMode && JumpAxis <= 0 && !IsCrouching)
		{
			//In flymode and jump is not being held
			if(Momentum.y > 0)
			{
				Momentum.y = Mathf.Clamp(Momentum.y - FlyFriction*Delta, 0, MaxVerticalSpeed);
			}
			else if(Momentum.y < 0)
			{
				Momentum.y = Mathf.Clamp(Momentum.y + FlyFriction*Delta, -MaxVerticalSpeed, 0);
			}
		}

		if(IsOnFloor() && !WasOnFloor && Abs(LastMomentumY) > SfxMinLandMomentumY)
		{
			float Volume = Abs(Clamp(LastMomentumY, -MaxVerticalSpeed, 0))/4 - 30;
			SfxManager.FpLand(Volume);
		}

		WasOnFloor = IsOnFloor();

		if(!IsJumping && (IsOnFloor() || FlyMode))
		{
			float SpeedLimit = BaseMovementSpeed;
			if(IsSprinting)
			{
				if(!FlyMode)
					SpeedLimit *= SprintMultiplyer;
				else if(FlyMode)
					SpeedLimit *= FlySprintMultiplyer;
			}
			else if(IsCrouching)
				SpeedLimit = BaseMovementSpeed/CrouchMovementDivisor;

			float X = 0, Z = 0;
			if(RightAxis > 0)
				X = -RightSens;
			else if(RightAxis < 0)
				X = LeftSens;
			if(ForwardAxis > 0)
				Z = ForwardSens;
			else if(ForwardAxis < 0)
				Z = -BackwardSens;

			float Speed = Momentum.Flattened().Length();
			if(Speed > 0)
			{
				if(FlyMode)
					Speed = Clamp(Speed - FlyFriction*Delta, 0, Speed);
				else if(IsCrouching && Speed > SpeedLimit)
					Speed = Clamp(Speed - (Friction/SlideFrictionDivisor)*Delta, 0, Speed);
				else
					Speed = Clamp(Speed - Friction*Delta, 0, Speed);

				Vector3 HorzMomentum = Momentum.Flattened().Normalized() * Speed;
				Momentum.x = HorzMomentum.x;
				Momentum.z = HorzMomentum.z;
			}

			{
				Vector3 WishDir = ClampVec3(new Vector3(X, 0, Z), 0, 1) * SpeedLimit;
				WishDir = WishDir.Rotated(new Vector3(0,1,0), Deg2Rad(LookHorizontal));

				float Multiplyer = Clamp(SpeedLimit - Momentum.Flattened().Length(), 0, SpeedLimit) / SpeedLimit;
				WishDir *= Multiplyer;

				Momentum.x += WishDir.x;
				Momentum.z += WishDir.z;
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
			WishDir = WishDir.Rotated(new Vector3(0,1,0), Deg2Rad(LookHorizontal));
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
			             .Rotated(new Vector3(1,0,0), Mathf.Deg2Rad(LoopRotation(-ActualLookVertical)))
			             .Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(LoopRotation(LookHorizontal))),
			             new Vector3(0,1,0), true, 100, Mathf.Deg2Rad(60));

			MoveAndSlide(new Vector3(0,Momentum.y,0).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(LookHorizontal)), new Vector3(0,1,0), true, 100, Mathf.Deg2Rad(60))
				.Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(LoopRotation(-LookHorizontal)));
		}
		else
		{
			Momentum = MoveAndSlide(Momentum, new Vector3(0,1,0), true, 100, Mathf.Deg2Rad(60));

			if(GetSlideCount() > 0)
			{
				Game.Mode.OnPlayerCollide(GetSlideCollision(0));
			}

			if(JumpAxis > 0 && !HasJumped && IsOnWall() && GetSlideCount() > 0 && !Ads && Game.Mode.ShouldWallKick())
			{
				HasJumped = true;

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

		{
			Items.ID ItemId;
			if(Inventory[InventorySlot] != null)
				ItemId = Inventory[InventorySlot].Id;
			else
				ItemId = Items.ID.ERROR;

			Net.SteelRpcUnreliable(this, nameof(Update), Translation, RotationDegrees, ActualLookVertical, IsJumping, Health, ItemId,
			                       Momentum.Rotated(new Vector3(0,1,0), Deg2Rad(LoopRotation(-LookHorizontal))).z);
		}

		if(!World.GetChunkTuple(Translation).Equals(CurrentChunk))
		{
			CurrentChunk = World.GetChunkTuple(Translation);
			Net.UnloadAndRequestChunks();
		}
	}


	[Remote]
	public void Update(Vector3 Position, Vector3 Rotation, float HeadRotation, bool Jumping, float Hp, Items.ID ItemId, float ForwardMomentum)
	{
		Health = Hp;

		if(Game.Mode.ShouldSyncRemotePlayerPosition(Id, Position))
		{
			Translation = Position;
		}

		if(Game.Mode.ShouldSyncRemotePlayerRotation(Id, Rotation))
		{
			RotationDegrees = Rotation;
		}

		HeadJoint.RotationDegrees = new Vector3(-HeadRotation, 0, 0);
		LegsJoint.RotationDegrees = new Vector3(Clamp((ForwardMomentum/MaxMovementSpeed)*MaxGroundLegRotation, -MaxAirLegRotation, MaxAirLegRotation), 0, 0);

		if(Round(ForwardMomentum) == 0 && !Jumping)
		{
			RightLegFlames.Emitting = false;
			LeftLegFlames.Emitting = false;
		}
		else
		{
			RightLegFlames.Emitting = true;
			LeftLegFlames.Emitting = true;
		}

		if(ItemId == Items.ID.ERROR)
			ThirdPersonItem.Hide();
		else
		{
			ThirdPersonItem.Mesh = Items.Meshes[ItemId];
			(ThirdPersonItem.MaterialOverride as ShaderMaterial).SetShaderParam("texture_albedo", Items.Textures[ItemId]);
			ThirdPersonItem.Show();
		}
	}


	[Remote]
	public void NotifyTeamChange(int NewTeam)
	{
		__team = NewTeam;
	}


	public override void _Process(float Delta)
	{
		Assert(MinAdsMultiplyer > 0 && MinAdsMultiplyer <= 1);
		if(Ads)
			AdsMultiplyer = Clamp(AdsMultiplyer - (Delta*(1-MinAdsMultiplyer)/AdsTime), MinAdsMultiplyer, 1);
		else
			AdsMultiplyer = Clamp(AdsMultiplyer + (Delta*(1-MinAdsMultiplyer)/AdsTime), MinAdsMultiplyer, 1);
		Cam.Fov = Game.Fov*AdsMultiplyer;

		ApplyLookVertical(0);
		var ToRemove = new List<Hitscan.AdditiveRecoil>();
		foreach(Hitscan.AdditiveRecoil Instance in ActiveAdditiveRecoil)
		{
			Instance.Life += Delta;
			if(Instance.Life > Instance.Length)
				ToRemove.Add(Instance);
		}
		foreach(Hitscan.AdditiveRecoil Instance in ToRemove)
			ActiveAdditiveRecoil.Remove(Instance);

		if(Inventory[InventorySlot] != null)
		{
			Items.ID Id = Inventory[InventorySlot].Id;
			ViewmodelItem.Mesh = Items.Meshes[Id];

			ShaderMaterial Mat = new ShaderMaterial();
			Mat.Shader = Items.TileShader;
			Mat.SetShaderParam("texture_albedo", Items.Textures[Id]);
			ViewmodelItem.MaterialOverride = Mat;

			ViewmodelItem.Show();

			{
				Items.IdInfo Info = Items.IdInfos[Inventory[InventorySlot].Id];
				if(IsPrimaryFiring && CurrentCooldown >= CurrentMaxCooldown && Info.UseDelegate != null && Info.FullAuto)
				{
					Items.UseItem(Inventory[InventorySlot], this);
				}
			}
		}
		else
		{
			ViewmodelItem.Hide();
		}
	}
}
