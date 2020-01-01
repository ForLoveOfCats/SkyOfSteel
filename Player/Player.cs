using Godot;
using static Godot.Mathf;
using static SteelMath;
using System;
using System.Collections.Generic;
using static System.Diagnostics.Debug;


public class Player : Character, IPushable, IInventory
{
	public bool Possessed = false;
	public int Id = 0;

	public float Height = 10;
	public float RequiredUncrouchHeight = 11;
	public float MovementSpeed = 36;
	public float FlySprintMultiplier = 5; //Speed while sprint flying is base speed times this value
	public float CrouchMovementDivisor = 2.8f;
	public float MaxVerticalSpeed = 100f;
	public float AirAcceleration = 25; //How many units per second to accelerate
	public float DecelerateTime = 0.1f; //How many seconds needed to stop from full speed
	public float Friction { get { return MovementSpeed / DecelerateTime; } }
	public float SlideFrictionDivisor = 13;
	public float FlyDecelerateTime = 0.15f; //How many seconds needed to stop from full speed
	public float FlyFriction { get { return (MovementSpeed*FlySprintMultiplier) / FlyDecelerateTime; } }
	public float JumpStartForce = 22f;
	public float JumpContinueForce = 0.41f;
	public float MaxJumpLength = 0.22f;
	public float Gravity = 55f;
	public float ItemThrowPower = 40f;
	public float ItemPickupDistance = 8f;
	public float SlotSwitchCooldown = 15;
	public float BuildingCooldown = 15;
	public float MaxGroundLegRotation = 50;
	public float MaxAirLegRotation = 80;
	public float MaxHealth = 100;
	public float LookDivisor = 6;
	public float ViewmodelMomentumMax = 12; //Probably never reaches this max
	public float ViewmodelMomentumHorzInputMultiplier = 0.9f;
	public float ViewmodelMomentumVertInputMultiplier = 0.9f;

	public static float AdsMultiplierMovementEffect = 1.66f;
	public static float MinAdsMultiplier = 0.7f;
	public static float AdsTime = 0.15f; //Seconds to achieve full ads

	public bool Ads = false;
	public float AdsMultiplier = 1;

	private const float SfxMinLandMomentumY = 3;

	private bool Frozen = true;
	public bool FlyMode { get; private set;} = false;

	public System.Tuple<int, int> CurrentChunk = new System.Tuple<int, int>(0, 0);

	public float Health = 0;
	private int __team = 1;
	public int Team
	{
		get => __team;
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
	public int CrouchAxis = 0;

	public float ForwardSens = 0;
	public float BackwardSens = 0;
	public float RightSens = 0;
	public float LeftSens = 0;
	public float FlySprintSens = 0;
	public float JumpSens = 0;

	public Vector2 ViewmodelMomentum = new Vector2();

	public bool IsCrouching = false;
	public bool IsFlySprinting = false;
	public bool IsJumping = false;
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

	public Items.Instance[] Inventory { get; set; } = new Items.Instance[10];
	public int InventorySlot = 0;

	public int BuildRotation = 0;

	public float NetUpdateDelta { get; private set; } = 0;

	public Camera Cam;
	public MeshInstance ViewmodelItem;
	public Position3D ViewmodelTiltJoint;
	public Position3D ViewmodelArmJoint;
	public float NormalViewmodelArmX = 0;
	public Spatial ProjectileEmitterHinge;
	public Spatial ProjectileEmitter;

	public CollisionShape LargeCollisionCapsule;
	public CollisionShape SmallCollisionCapsule;

	public Spatial HeadJoint;
	public Spatial LegsJoint;
	public MeshInstance ThirdPersonItem;

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

		ViewmodelItem = GetNode<MeshInstance>("SteelCamera/ViewmodelArmJoint/ViewmodelTiltJoint/ViewmodelItem");
		ViewmodelItem.RotationDegrees = new Vector3(0, 180, 0);
		ViewmodelItem.Hide();
		ViewmodelTiltJoint = GetNode<Position3D>("SteelCamera/ViewmodelArmJoint/ViewmodelTiltJoint");
		ViewmodelArmJoint = GetNode<Position3D>("SteelCamera/ViewmodelArmJoint");
		ViewmodelArmJoint.RotationDegrees = new Vector3();
		NormalViewmodelArmX = ViewmodelArmJoint.Translation.x;
		ViewmodelArmJoint.Translation = new Vector3(NormalViewmodelArmX, ViewmodelArmJoint.Translation.y, ViewmodelArmJoint.Translation.z);

		ProjectileEmitterHinge = GetNode<Spatial>("ProjectileEmitterHinge");
		ProjectileEmitter = GetNode<Spatial>("ProjectileEmitterHinge/ProjectileEmitter");

		LargeCollisionCapsule = GetNode<CollisionShape>("LargeCollisionShape");
		SmallCollisionCapsule = GetNode<CollisionShape>("SmallCollisionShape");

		if(Possessed)
		{
			Cam.MakeCurrent();
			GetNode<RayCast>("SteelCamera/RayCast").AddException(this);
			GetNode<Spatial>("BodyScene").Free();

			AddChild(HUDInstance);

			GhostInstance = (Ghost) GD.Load<PackedScene>("res://World/Ghost.tscn").Instance();
			GhostInstance.Hide();
			GetParent().CallDeferred("add_child", GhostInstance);

			SfxManager = GetNode<PlayerSfxManager>("PlayerSfxManager");
		}
		else
		{
			HeadJoint = GetNode("BodyScene").GetNode<Spatial>("HeadJoint");
			LegsJoint = GetNode("BodyScene").GetNode<Spatial>("LegsJoint");

			RightLegFlames = GetNode("BodyScene").GetNode<CPUParticles>("LegsJoint/RightLegFlames");
			LeftLegFlames = GetNode("BodyScene").GetNode<CPUParticles>("LegsJoint/LeftLegFlames");

			ThirdPersonItem = GetNode("BodyScene").GetNode<MeshInstance>("ItemMesh");
			ShaderMaterial Mat = new ShaderMaterial();
			Mat.Shader = Items.TileShader;
			ThirdPersonItem.MaterialOverride = Mat;

			Spatial Body = GetNode<Spatial>("BodyScene");
			Body.GetNode<HitboxClass>("BodyHitbox").OwningPlayer = this;
			Body.GetNode<HitboxClass>("HeadJoint/HeadHitbox").OwningPlayer = this;
			Body.GetNode<HitboxClass>("LegsJoint/LegsHitbox").OwningPlayer = this;

			return;
		}

		Respawn();

		if(Net.Work.IsNetworkServer())
		{
			SetFreeze(false);
			GiveDefaultItems();
		}
	}


	public void GiveDefaultItems()
	{
		ItemGive(new Items.Instance(Items.ID.PLATFORM));
		ItemGive(new Items.Instance(Items.ID.WALL));
		ItemGive(new Items.Instance(Items.ID.SLOPE));
		ItemGive(new Items.Instance(Items.ID.TRIANGLE_WALL));
		ItemGive(new Items.Instance(Items.ID.PIPE));
		ItemGive(new Items.Instance(Items.ID.PIPE_JOINT));
		ItemGive(new Items.Instance(Items.ID.LOCKER));
		ItemGive(new Items.Instance(Items.ID.ROCKET_JUMPER));
		ItemGive(new Items.Instance(Items.ID.THUNDERBOLT));
		ItemGive(new Items.Instance(Items.ID.SCATTERSHOCK));
		// ItemGive(new Items.Instance(Items.ID.SWIFTSPARK));
	}


	[Remote]
	public void SetFreeze(bool NewFrozen)
	{
		if(Possessed)
			Frozen = NewFrozen;
		else
		{
			Frozen = NewFrozen;
			RpcId(Id, nameof(SetFreeze), NewFrozen);
		}
	}


	public void SetFly(bool NewFly) //because custom setters are weird
	{
		FlyMode = NewFly;
		Momentum = new Vector3(0,0,0);
	}


	[SteelInputWithoutArg(typeof(Player), nameof(ToggleFly))]
	public static void ToggleFly()
	{
		Player Plr = Game.PossessedPlayer;
		Plr.SetFly(!Plr.FlyMode);
	}


	public void ApplyPush(Vector3 Push)
	{
		if(!FlyMode)
			Momentum += Push;
	}


	public void MovementReset()
	{
		Translation = new Vector3(0, 5.1f + 0.15f, 0);
		Momentum = new Vector3();
	}


	[SteelInputWithoutArg(typeof(Player), nameof(InputRespawn))]
	public static void InputRespawn()
	{
		Game.PossessedPlayer.Respawn();
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


	[Remote]
	public void NotifyPickedUpItem()
	{
		if(!Possessed)
		{
			Assert(Net.Work.IsNetworkServer());
			Net.SteelRpc(this, nameof(NotifyPickedUpItem));
			return;
		}

		SfxManager.FpPickup();
		SetCooldown(0, SlotSwitchCooldown, false);
	}


	public void ItemGive(Items.Instance ToGive)
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception("Attempted to give item on client");

		for(int Slot = 0; Slot <= 9; Slot++)
		{
			if(!(Inventory[Slot] is null)) //If inventory item is not null
			{
				if(Inventory[Slot].Id == ToGive.Id)
				{
					Inventory[Slot].Count += ToGive.Count;

					if(Possessed)
						HUDInstance.HotbarUpdate();
					else
						RpcId(Id, nameof(NetUpdateInventorySlot), Slot, ToGive.Id, Inventory[Slot].Count);

					return;
				}
			}
		}

		for(int Slot = 0; Slot <= 9; Slot++)
		{
			if(Inventory[Slot] is null)
			{
				Inventory[Slot] = ToGive;

				if(Possessed)
					HUDInstance.HotbarUpdate();
				else
					RpcId(Id, nameof(NetUpdateInventorySlot), Slot, ToGive.Id, ToGive.Count);

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


	[SteelInputWithoutArg(typeof(Player), nameof(InventoryUp))]
	public static void InventoryUp()
	{
		Player Plr = Game.PossessedPlayer;
		if(!(Plr.CurrentCooldown < Plr.CurrentMaxCooldown && Plr.PreventSwitch))
		{
			Plr.BuildRotation = 0;

			Plr.InventorySlot--;
			if(Plr.InventorySlot < 0)
			{
				Plr.InventorySlot = 9;
			}

			Plr.HUDInstance.HotbarUpdate();
			Hitscan.Reset();
			Plr.SetCooldown(0, Plr.SlotSwitchCooldown, false);
			Plr.Ads = false;
		}
	}


	[SteelInputWithoutArg(typeof(Player), nameof(InventoryDown))]
	public static void InventoryDown()
	{
		Player Plr = Game.PossessedPlayer;
		if(!(Plr.CurrentCooldown < Plr.CurrentMaxCooldown && Plr.PreventSwitch))
		{
			Plr.BuildRotation = 0;

			Plr.InventorySlot++;
			if(Plr.InventorySlot > 9)
			{
				Plr.InventorySlot = 0;
			}

			Plr.HUDInstance.HotbarUpdate();
			Hitscan.Reset();
			Plr.SetCooldown(0, Plr.SlotSwitchCooldown, false);
			Plr.Ads = false;
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


	[SteelInputWithoutArg(typeof(Player), nameof(InventorySlot0))]
	public static void InventorySlot0() { Game.PossessedPlayer.InventorySlotSelect(0); }

	[SteelInputWithoutArg(typeof(Player), nameof(InventorySlot1))]
	public static void InventorySlot1() { Game.PossessedPlayer.InventorySlotSelect(1); }

	[SteelInputWithoutArg(typeof(Player), nameof(InventorySlot2))]
	public static void InventorySlot2() { Game.PossessedPlayer.InventorySlotSelect(2); }

	[SteelInputWithoutArg(typeof(Player), nameof(InventorySlot3))]
	public static void InventorySlot3() { Game.PossessedPlayer.InventorySlotSelect(3); }

	[SteelInputWithoutArg(typeof(Player), nameof(InventorySlot4))]
	public static void InventorySlot4() { Game.PossessedPlayer.InventorySlotSelect(4); }

	[SteelInputWithoutArg(typeof(Player), nameof(InventorySlot5))]
	public static void InventorySlot5() { Game.PossessedPlayer.InventorySlotSelect(5); }

	[SteelInputWithoutArg(typeof(Player), nameof(InventorySlot6))]
	public static void InventorySlot6() { Game.PossessedPlayer.InventorySlotSelect(6); }

	[SteelInputWithoutArg(typeof(Player), nameof(InventorySlot7))]
	public static void InventorySlot7() { Game.PossessedPlayer.InventorySlotSelect(7); }

	[SteelInputWithoutArg(typeof(Player), nameof(InventorySlot8))]
	public static void InventorySlot8() { Game.PossessedPlayer.InventorySlotSelect(8); }

	[SteelInputWithoutArg(typeof(Player), nameof(InventorySlot9))]
	public static void InventorySlot9() { Game.PossessedPlayer.InventorySlotSelect(9); }


	[SteelInputWithArg(typeof(Player), nameof(BuildRotate))]
	public static void BuildRotate(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		if(Sens > 0 && Plr.Inventory[Plr.InventorySlot] != null)
		{
			Plr.BuildRotation++;
			if(Plr.BuildRotation > 3)
				Plr.BuildRotation = 0;
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(ForwardMove))]
	public static void ForwardMove(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		Plr.ForwardSens = Sens;
		if(Sens > 0)
		{
			Plr.ForwardAxis = 1;
		}
		else if(Plr.ForwardAxis > 0)
		{
			Plr.ForwardAxis = 0;
			if(Plr.BackwardSens > 0)
			{
				Plr.ForwardAxis = -1;
			}
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(BackwardMove))]
	public static void BackwardMove(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		Plr.BackwardSens = Sens;
		if(Sens > 0)
		{
			Plr.ForwardAxis = -1;
		}
		else if(Plr.ForwardAxis < 0)
		{
			Plr.ForwardAxis = 0;
			if(Plr.ForwardSens > 0)
			{
				Plr.ForwardAxis = 1;
			}
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(RightMove))]
	public static void RightMove(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		Plr.RightSens = Sens;
		if(Sens > 0)
		{
			Plr.RightAxis = 1;
		}
		else if(Plr.RightAxis > 0)
		{
			Plr.RightAxis = 0;
			if(Plr.LeftSens > 0)
			{
				Plr.RightAxis = -1;
			}
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(LeftMove))]
	public static void LeftMove(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		Plr.LeftSens = Sens;
		if(Sens > 0)
		{
			Plr.RightAxis = -1;
		}
		else if(Plr.RightAxis < 0)
		{
			Plr.RightAxis = 0;
			if(Plr.RightSens >0)
			{
				Plr.RightAxis = 1;
			}
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(FlySprint))]
	public static void FlySprint(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		Plr.FlySprintSens = Sens;
		if(Sens > 0 && Plr.FlyMode)
		{
			Plr.IsFlySprinting = true;

			if(Plr.JumpAxis == 1)
				Plr.Momentum.y = Plr.MovementSpeed*Plr.FlySprintMultiplier;
			else if(Plr.IsCrouching)
				Plr.Momentum.y = -Plr.MovementSpeed*Plr.FlySprintMultiplier;
		}
		else
		{
			Plr.IsFlySprinting = false;
			Plr.Momentum.y = Clamp(Plr.Momentum.y, -Plr.MovementSpeed, Plr.MovementSpeed);
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(Jump))]
	public static void Jump(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		Plr.JumpSens = Sens;
		if(Sens > 0)
		{
			if(Plr.FlyMode)
			{
				if(Plr.IsFlySprinting)
				{
					Plr.Momentum.y = Plr.MovementSpeed*Plr.FlySprintMultiplier;
				}
				else
				{
					Plr.Momentum.y = Plr.MovementSpeed;
				}
				Plr.IsJumping = false;
			}
			else if(Plr.OnFloor)
			{
				Plr.Momentum.y = Plr.JumpStartForce;
				Plr.IsJumping = true;
			}

			Plr.JumpAxis = 1;
		}
		else
		{
			Plr.JumpAxis = 0;
			Plr.IsJumping = false;
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(Crouch))]
	public static void Crouch(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		if(Sens > 0)
		{
			Plr.CrouchAxis = 1;
			Plr.IsCrouching = true;

			if(!Plr.FlyMode)
				Plr.IsFlySprinting = false;

			if(Plr.FlyMode)
			{
				Plr.JumpAxis = 0;
				Plr.JumpSens = 0;

				if(Plr.IsFlySprinting)
					Plr.Momentum.y = -Plr.MovementSpeed*Plr.FlySprintMultiplier;
				else
					Plr.Momentum.y = -Plr.MovementSpeed;
			}

			Plr.LargeCollisionCapsule.Disabled = true;
			Plr.SmallCollisionCapsule.Disabled = false;
		}
		else
		{
			Plr.CrouchAxis = 0;

			if(Plr.FlySprintSens > 0)
				FlySprint(Plr.FlySprintSens);
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
		Cam.RotationDegrees = new Vector3(ActualLookVertical, 180, 0);
		ProjectileEmitterHinge.RotationDegrees = new Vector3(ActualLookVertical, 180, 0);
	}


	public float CalcViewmodelMomentumChange(float Sens)
	{
		return ((float)Math.Log10(Sens+1)) * 3f * AdsMultiplier;
	}


	[SteelInputWithArg(typeof(Player), nameof(LookUp))]
	public static void LookUp(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		if(Sens > 0)
		{
			float Change = ((float)Sens/Plr.LookDivisor)*Game.LookSensitivity*Plr.AdsMultiplier;

			Plr.ApplyLookVertical(Change);

			Plr.ViewmodelMomentum = new Vector2(
				Plr.ViewmodelMomentum.x,
				Clamp(Plr.ViewmodelMomentum.y - Plr.CalcViewmodelMomentumChange(Sens)*Plr.ViewmodelMomentumVertInputMultiplier, -Plr.ViewmodelMomentumMax, Plr.ViewmodelMomentumMax)
			);
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(LookDown))]
	public static void LookDown(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		if(Sens > 0)
		{
			float Change = ((float)Sens/Plr.LookDivisor)*Game.LookSensitivity*Plr.AdsMultiplier;

			Plr.ApplyLookVertical(-Change);

			Plr.ViewmodelMomentum = new Vector2(
				Plr.ViewmodelMomentum.x,
				Clamp(Plr.ViewmodelMomentum.y + Plr.CalcViewmodelMomentumChange(Sens)*Plr.ViewmodelMomentumVertInputMultiplier, -Plr.ViewmodelMomentumMax, Plr.ViewmodelMomentumMax)
			);
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(LookRight))]
	public static void LookRight(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		if(Sens > 0)
		{
			float Change = ((float)Sens/Plr.LookDivisor)*Game.LookSensitivity*Plr.AdsMultiplier;

			Plr.LookHorizontal -= Change;
			Plr.RotationDegrees = new Vector3(0, Plr.LookHorizontal, 0);

			Plr.ViewmodelMomentum = new Vector2(
				Clamp(Plr.ViewmodelMomentum.x + Plr.CalcViewmodelMomentumChange(Sens)*Plr.ViewmodelMomentumHorzInputMultiplier, -Plr.ViewmodelMomentumMax, Plr.ViewmodelMomentumMax),
				Plr.ViewmodelMomentum.y
			);
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(LookLeft))]
	public static void LookLeft(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		if(Sens > 0)
		{
			float Change = ((float)Sens/Plr.LookDivisor)*Game.LookSensitivity*Plr.AdsMultiplier;

			Plr.LookHorizontal += Change;
			Plr.RotationDegrees = new Vector3(0, Plr.LookHorizontal, 0);

			Plr.ViewmodelMomentum = new Vector2(
				Clamp(Plr.ViewmodelMomentum.x - Plr.CalcViewmodelMomentumChange(Sens)*Plr.ViewmodelMomentumHorzInputMultiplier, -Plr.ViewmodelMomentumMax, Plr.ViewmodelMomentumMax),
				Plr.ViewmodelMomentum.y
			);
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(PrimaryFire))]
	public static void PrimaryFire(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		if(Sens > 0 && !Plr.IsPrimaryFiring && Plr.CurrentCooldown >= Plr.CurrentMaxCooldown)
		{
			Plr.IsPrimaryFiring = true;

			if(Plr.Inventory[Plr.InventorySlot] != null)
			{
				if(Items.IdInfos[Plr.Inventory[Plr.InventorySlot].Id].PositionDelegate != null)
				{
					var BuildRayCast = Plr.GetNode<RayCast>("SteelCamera/RayCast");
					if(BuildRayCast.IsColliding())
					{
						if(BuildRayCast.GetCollider() is Tile Base && Plr.GhostInstance.CanBuild)
						{
							Vector3? PlacePosition = Items.TryCalculateBuildPosition(
								Plr.GhostInstance.CurrentMeshType,
								Base, Plr.RotationDegrees.y,
								Plr.BuildRotation,
								BuildRayCast.GetCollisionPoint()
							);

							if(PlacePosition != null)
							{
								World.PlaceOn(
									Plr.GhostInstance.CurrentMeshType,
									Base,
									Plr.RotationDegrees.y,
									Plr.BuildRotation,
									BuildRayCast.GetCollisionPoint(),
									1 //ID 1 for now so all client own all non-default structures
								);
								Plr.SetCooldown(0, Plr.BuildingCooldown, true);
							}
						}
					}
				}

				if(Items.IdInfos[Plr.Inventory[Plr.InventorySlot].Id].UseDelegate != null)
				{
					Items.UseItem(Plr.Inventory[Plr.InventorySlot], Plr);
				}
			}
		}

		if(Sens <= 0 && Plr.IsPrimaryFiring)
		{
			Plr.IsPrimaryFiring = false;
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(SecondaryFire))]
	public static void SecondaryFire(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		if(Sens > 0 && !Plr.IsSecondaryFiring)
		{
			Plr.IsSecondaryFiring = true;

			Items.Instance CurrentItem = Plr.Inventory[Plr.InventorySlot];

			if(CurrentItem == null || !Items.IdInfos[CurrentItem.Id].CanAds)
			{
				if(Plr.CurrentCooldown >= Plr.CurrentMaxCooldown)
				{
					RayCast BuildRayCast = Plr.GetNode<RayCast>("SteelCamera/RayCast");
					if(BuildRayCast.IsColliding())
					{
						if(BuildRayCast.GetCollider() is Tile Hit)
						{
							Hit.NetRemove();
							Plr.SetCooldown(0, Plr.BuildingCooldown, true);
						}
					}
				}
			}

			else if(CurrentItem != null && Items.IdInfos[CurrentItem.Id].CanAds)
			{
				Plr.Ads = true;
				Plr.IsFlySprinting = false;
			}
		}

		if(Sens <= 0 && Plr.IsSecondaryFiring)
		{
			Plr.IsSecondaryFiring = false;

			Items.Instance CurrentItem = Plr.Inventory[Plr.InventorySlot];
			if(CurrentItem != null && Items.IdInfos[CurrentItem.Id].CanAds)
			{
				Plr.Ads = false;
				if(Plr.FlySprintSens > 0)
					FlySprint(Plr.FlySprintSens);
			}
		}
	}


	[SteelInputWithArg(typeof(Player), nameof(ThrowCurrentItem))]
	public static void ThrowCurrentItem(float Sens)
	{
		Player Plr = Game.PossessedPlayer;
		if(Sens > 0)
		{
			Vector3 Vel = Plr.Momentum/1.5f + new Vector3(0, 0, Plr.ItemThrowPower)
				.Rotated(new Vector3(1,0,0), Deg2Rad(-Plr.ActualLookVertical))
				.Rotated(new Vector3(0,1,0), Deg2Rad(Plr.LookHorizontal));

			if(Net.Work.IsNetworkServer())
				Plr.ThrowItemFromSlot(Plr.InventorySlot, Vel);
			else
			{
				Plr.RpcId(Net.ServerId, nameof(ThrowItemFromSlot), Plr.InventorySlot, Vel);

				if(Plr.Inventory[Plr.InventorySlot] != null)
				{
					Plr.SfxManager.FpThrow();
					Plr.SetCooldown(0, Plr.SlotSwitchCooldown, false);
				}
			}
		}
	}


	[Remote]
	public void ThrowItemFromSlot(int Slot, Vector3 Vel)
	{
		if(Inventory[Slot] != null)
		{
			World.Self.DropItem(Inventory[Slot].Id, Translation+Cam.Translation, Vel);

			if(Inventory[Slot].Count > 1)
			{
				Inventory[Slot].Count -= 1;

				if(Id != Net.Work.GetNetworkUniqueId())
					RpcId(Id, nameof(NetUpdateInventorySlot), Slot, Inventory[Slot].Id, Inventory[Slot].Count);
			}
			else
			{
				Inventory[Slot] = null;

				if(Id != Net.Work.GetNetworkUniqueId())
					RpcId(Id, nameof(NetEmptyInventorySlot), Slot);
			}

			if(Id == Net.Work.GetNetworkUniqueId())
			{
				HUDInstance.HotbarUpdate();
				SfxManager.FpThrow();
				SetCooldown(0, SlotSwitchCooldown, false);
			}
		}
	}


	public float GetAdsMovementMultiplyer()
	{
		return Clamp(((AdsMultiplier-1) * AdsMultiplierMovementEffect)+1, 0, 1);
	}


	public Vector3 AirAccelerate(Vector3 Vel, Vector3 WishDir, float Delta)
	{
		WishDir = ClampVec3(WishDir, 0, 1) * (AirAcceleration*GetAdsMovementMultiplyer());
		float CurrentSpeed = Vel.Dot(WishDir);
		float AddSpeed = AirAcceleration*GetAdsMovementMultiplyer() - CurrentSpeed;
		AddSpeed = Clamp(AddSpeed, 0, AirAcceleration*GetAdsMovementMultiplyer()*Delta);
		return Vel + WishDir * AddSpeed;
	}


	public override void _PhysicsProcess(float Delta)
	{
		if(Frozen)
			return;

		if(Net.Work.IsNetworkServer())
		{
			List<DroppedItem> ToPickUpList = new List<DroppedItem>();
			foreach(DroppedItem Item in World.ItemList)
			{
				if(Translation.DistanceTo(Item.Translation) <= ItemPickupDistance && Item.Life >= DroppedItem.MinPickupLife)
				{
					PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;
					Godot.Collections.Dictionary Results = State.IntersectRay(Translation, Item.Translation, new Godot.Collections.Array{this}, 4);
					if(Results.Count <= 0)
						ToPickUpList.Add(Item);
				}
			}
			if(ToPickUpList.Count > 0)
			{
				foreach(DroppedItem Item in ToPickUpList)
				{
					Player Plr = Net.Players[Id];
					Plr.ItemGive(new Items.Instance(Item.Type));
					Plr.NotifyPickedUpItem();

					Net.SteelRpc(World.Self, nameof(World.RemoveDroppedItem), Item.Name);
					World.Self.RemoveDroppedItem(Item.Name);
				}
			}
		}

		if(!Possessed)
			return;

		if(Health <= 0)
			Respawn();

		CurrentCooldown = Clamp(CurrentCooldown + (100*Delta), 0, CurrentMaxCooldown);

		if(JumpAxis > 0 && OnFloor && !IsCrouching && !Ads)
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

		if(!OnFloor && !IsJumping && !FlyMode)
			Momentum.y = Clamp(Momentum.y - Gravity*Delta, -MaxVerticalSpeed, MaxVerticalSpeed);

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

		if(OnFloor && !WasOnFloor && Abs(LastMomentumY) > SfxMinLandMomentumY)
		{
			float Volume = Abs(Clamp(LastMomentumY, -MaxVerticalSpeed, 0))/4 - 30;
			SfxManager.FpLand(Volume);
		}

		WasOnFloor = OnFloor;

		if(!IsJumping && (OnFloor || FlyMode))
		{
			float SpeedLimit = MovementSpeed*GetAdsMovementMultiplyer();
			if(FlyMode && IsFlySprinting)
				SpeedLimit *= FlySprintMultiplier;
			else if(IsCrouching)
				SpeedLimit = (MovementSpeed*GetAdsMovementMultiplyer())/CrouchMovementDivisor;

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

				float Multiplier = Clamp(SpeedLimit - Momentum.Flattened().Length(), 0, SpeedLimit) / SpeedLimit;
				WishDir *= Multiplier;

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
			Momentum = Move(Momentum, Delta, 2, 60f, MovementSpeed);

		if(IsCrouching && CrouchAxis == 0)
		{
			PhysicsDirectSpaceState State = GetWorld().DirectSpaceState;

			Godot.Collections.Dictionary DownResults = State.IntersectRay(Translation, Translation - new Vector3(0, Height, 0), new Godot.Collections.Array{this}, 1);
			Godot.Collections.Dictionary UpResults = State.IntersectRay(Translation, Translation + new Vector3(0, Height, 0), new Godot.Collections.Array{this}, 1);

			bool UnCrouch = true;
			if(DownResults.Count > 0 && UpResults.Count > 0)
			{
				float DownY = ((Vector3)DownResults["position"]).y;
				float UpY = ((Vector3)UpResults["position"]).y;

				if(UpY - DownY <= RequiredUncrouchHeight)
					UnCrouch = false;
			}

			if(UnCrouch)
			{
				IsCrouching = false;
				SmallCollisionCapsule.Disabled = true;
				LargeCollisionCapsule.Disabled = false;

				if(DownResults.Count > 0)
				{
					float DownY = ((Vector3)DownResults["position"]).y;
					if(Translation.y - DownY <= Height/2)
						Translation = new Vector3(Translation.x, DownY + (Height/2), Translation.z);
				}
			}
		}

		{
			Items.ID ItemId;
			if(Inventory[InventorySlot] != null)
				ItemId = Inventory[InventorySlot].Id;
			else
				ItemId = Items.ID.ERROR;

			Net.SteelRpcUnreliable(this, nameof(Update), this.Transform, ActualLookVertical, IsJumping, IsCrouching, Health, ItemId,
			                       Momentum.Rotated(new Vector3(0,1,0), Deg2Rad(LoopRotation(-LookHorizontal))).z);
		}

		if(!World.GetChunkTuple(Translation).Equals(CurrentChunk))
		{
			CurrentChunk = World.GetChunkTuple(Translation);
			Net.UnloadAndRequestChunks();
		}
	}


	[Remote]
	public void Update(Transform TargetTransform, float HeadRotation, bool Jumping, bool Crouching, float Hp, Items.ID ItemId, float ForwardMomentum)
	{
		Health = Hp;

		this.Transform = this.Transform.InterpolateWith(
			TargetTransform,
			NetUpdateDelta*20
		);

		HeadJoint.Transform = HeadJoint.Transform.InterpolateWith(
			new Transform(
				new Quat(
					new Vector3(Deg2Rad(-HeadRotation),0,0)
				),
				HeadJoint.Transform.origin
			),
			NetUpdateDelta*20
		);

		float LegsJointTarget = 0;
		if(!Crouching)
			LegsJointTarget = Clamp((ForwardMomentum/MovementSpeed)*MaxGroundLegRotation, -MaxAirLegRotation, MaxAirLegRotation);
		else
			LegsJointTarget = MaxAirLegRotation*Sign(ForwardMomentum);
		LegsJoint.Transform = LegsJoint.Transform.InterpolateWith(
			new Transform(
				new Quat(
					new Vector3(Deg2Rad(LegsJointTarget),0,0)
				),
				LegsJoint.Transform.origin
			),
			NetUpdateDelta*20
		);

		if(RoundToInt(ForwardMomentum) == 0 && !Jumping)
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
			((ShaderMaterial)ThirdPersonItem.MaterialOverride).SetShaderParam("texture_albedo", Items.Textures[ItemId]);
			ThirdPersonItem.Show();
		}

		NetUpdateDelta = Single.Epsilon;
	}


	[Remote]
	public void NotifyTeamChange(int NewTeam)
	{
		__team = NewTeam;
	}


	[Remote]
	public void NetUpdateInventorySlot(int Slot, Items.ID Id, int Count)
	{
		Inventory[Slot] = new Items.Instance(Id) {Count = Count};
		HUDInstance.HotbarUpdate();
	}


	[Remote]
	public void NetEmptyInventorySlot(int Slot)
	{
		Inventory[Slot] = null;
		HUDInstance.HotbarUpdate();
	}


	public override void _Process(float Delta)
	{
		if(!Possessed)
		{
			NetUpdateDelta += Delta;
			return;
		}

		Assert(MinAdsMultiplier > 0 && MinAdsMultiplier <= 1);
		AdsMultiplier =
			Ads
				? Clamp(AdsMultiplier - (Delta * (1 - MinAdsMultiplier) / AdsTime), MinAdsMultiplier, 1)
				: Clamp(AdsMultiplier + (Delta * (1 - MinAdsMultiplier) / AdsTime), MinAdsMultiplier, 1);

		Cam.Fov = Game.Fov*AdsMultiplier;

		float Length = ViewmodelMomentum.Length();
		Vector2 Normalized = ViewmodelMomentum.Normalized();
		ViewmodelMomentum = Normalized * Clamp(Length - (Length*Delta*28f), 0, ViewmodelMomentumMax);

		ViewmodelItem.RotationDegrees = new Vector3(
			ViewmodelMomentum.y*AdsMultiplier*1.2f,
			180 - ViewmodelMomentum.x*AdsMultiplier*1.2f,
			0
		);
		ViewmodelArmJoint.RotationDegrees = new Vector3(
			ViewmodelMomentum.y*AdsMultiplier,
			ViewmodelMomentum.x*AdsMultiplier,
			0
		);
		ViewmodelArmJoint.Translation = new Vector3(
			NormalViewmodelArmX * ((AdsMultiplier-MinAdsMultiplier) * (1/(1-MinAdsMultiplier))),
			ViewmodelArmJoint.Translation.y,
			ViewmodelArmJoint.Translation.z
		);

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
