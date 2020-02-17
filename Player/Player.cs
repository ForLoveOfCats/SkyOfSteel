using Godot;
using Optional;
using System;
using System.Collections.Generic;
using static SteelMath;
using static Godot.Mathf;



public class Player : Character, IPushable, IHasInventory
{
	public const float Height = 10;
	public const float RequiredUncrouchHeight = 11;
	public const float MovementSpeed = 36;
	public const float FlySprintMultiplier = 5; //Speed while sprint flying is base speed times this value
	public const float CrouchMovementDivisor = 2.8f;
	public const float MaxVerticalSpeed = 100f;
	public const float AirAcceleration = 25; //How many units per second to accelerate
	public const float DecelerateTime = 0.1f; //How many seconds needed to stop from full speed
	public const float Friction = MovementSpeed / DecelerateTime;
	public const float SlideFrictionDivisor = 13;
	public const float FlyDecelerateTime = 0.15f; //How many seconds needed to stop from full speed
	public const float FlyFriction = MovementSpeed * FlySprintMultiplier / FlyDecelerateTime;
	public const float JumpStartForce = 22f;
	public const float JumpContinueForce = 0.41f;
	public const float MaxJumpLength = 0.22f;
	public const float Gravity = 55f;
	public const float InteractReach = 12f;
	public const float ItemThrowPower = 40f;
	public const float ItemPickupDistance = 8f;
	public const float SlotSwitchCooldown = 15;
	public const float BuildingCooldown = 15;
	public const float MaxGroundLegRotation = 50;
	public const float MaxAirLegRotation = 80;
	public const float MaxHealth = 100;
	public const float LookDivisor = 6;

	public const float ViewmodelSensitivity = 0.1f;
	public const float MaxViewmodelItemRotation = 12f;
	public const float MaxViewmodelArmRotation = 4f;

	public const float AdsMultiplierMovementEffect = 1.66f;
	public const float MinAdsMultiplier = 0.7f;
	public const float AdsTime = 0.15f; //Seconds to achieve full ads

	public const float SfxMinLandMomentumY = 3;

	public bool Possessed = false;
	public int Id = 0;

	public bool Ads = false;
	public float AdsMultiplier = 1;

	private bool Frozen = true;
	public bool FlyMode { get; private set;} = false;

	public System.Tuple<int, int> CurrentChunk = new System.Tuple<int, int>(0, 0);

	public float Health = 0;
	public bool Dying = false;

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

	public InventoryComponent Inventory { get; set; }
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

	private Player()
	{
		if(Engine.EditorHint) {return;}

		//Init eleven slots, only use ten of them. The eleventh is used for dropping
		Inventory = new InventoryComponent(this, 11, HiddenLast:true);

		HUDInstance = (HUD) GD.Load<PackedScene>("res://UI/HUD.tscn").Instance();
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

		Reset();

		if(Net.Work.IsNetworkServer())
		{
			SetFreeze(false);
			GiveDefaultItems();
		}
	}


	public void GiveDefaultItems()
	{
		ItemGive(new Items.Instance(Items.ID.PLATFORM) {Count = 50});
		ItemGive(new Items.Instance(Items.ID.WALL) {Count = 50});
		ItemGive(new Items.Instance(Items.ID.SLOPE) {Count = 50});
		ItemGive(new Items.Instance(Items.ID.TRIANGLE_WALL) {Count = 50});
		ItemGive(new Items.Instance(Items.ID.PIPE) {Count = 50});
		ItemGive(new Items.Instance(Items.ID.PIPE_JOINT) {Count = 50});
		ItemGive(new Items.Instance(Items.ID.LOCKER) {Count = 50});
		ItemGive(new Items.Instance(Items.ID.ROCKET_JUMPER) {Count = 50});
		ItemGive(new Items.Instance(Items.ID.THUNDERBOLT) {Count = 50});
		ItemGive(new Items.Instance(Items.ID.SCATTERSHOCK) {Count = 50});
		// ItemGive(new Items.Instance(Items.ID.SWIFTSPARK) {Count = 50});
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


	public void Reset()
	{
		HUDInstance.ClearDamageIndicators();
		MovementReset();
		Ads = false;
		Health = MaxHealth;
	}


	[Remote]
	public void Die()
	{
		if(!Net.Work.IsNetworkServer())
			RpcId(Net.ServerId, nameof(Die));
		else
		{
			Net.SteelRpc(this, nameof(NetDie));
			NetDie();
		}
	}


	[Remote]
	public void NetDie()
	{
		if(Possessed)
		{
			Cam.ClearCurrent(false);
			Game.PossessedPlayer = Player.None();
			World.UnloadAndRequestChunks(new Vector3(), 0);
			Menu.BuildPause();
		}

		Net.Players[Id].Plr = Player.None();
		QueueFree();
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
			Assert.ActualAssert(Net.Work.IsNetworkServer());
			Net.SteelRpc(this, nameof(NotifyPickedUpItem));
			return;
		}

		SfxManager.FpPickup();
		SetCooldown(0, SlotSwitchCooldown, false);
	}


	public Option<int[]> ItemGive(Items.Instance ToGive)
	{
		Option<int[]> Slots = Inventory.Give(ToGive);

		Slots.MatchSome(
			(ActualSlots) =>
			{
				if(Possessed)
					HUDInstance.HotbarUpdate();
				else
				{
					foreach(int Slot in ActualSlots)
						RpcId(Id, nameof(NetUpdateInventorySlot), Slot, ToGive.Id, Inventory[Slot].Count);
				}
			}
		);

		return Slots;
	}


	public void SetCooldown(float NewCooldown, float NewMaxCooldown, bool NewPreventSwitch)
	{
		CurrentCooldown = Clamp(NewCooldown, 0, NewMaxCooldown);
		CurrentMaxCooldown = NewMaxCooldown;
		PreventSwitch = NewPreventSwitch;
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


	public Vector3 CalcThrowVelocity()
	{
		float Magnitude = ItemThrowPower + (float)Game.Rand.NextDouble() * Game.Rand.RandomSign();
		float VDiff = (float)(Game.Rand.NextDouble() * 2d) * Game.Rand.RandomSign();
		float HDiff = (float)(Game.Rand.NextDouble() * 2d) * Game.Rand.RandomSign();

		Vector3 Vel = Momentum/1.5f + new Vector3(0, 0, Magnitude)
			.Rotated(new Vector3(1,0,0), Deg2Rad(-ActualLookVertical + VDiff))
			.Rotated(new Vector3(0,1,0), Deg2Rad(LookHorizontal + HDiff));

		return Vel;
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
				Inventory.EmptySlot((Slot));

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

		if(Possessed && !Dying && Health <= 0)
		{
			Die();
			Dying = true;
		}

		if(Dying)
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
					Net.Players[Id].Plr.MatchSome(
						(Plr) =>
						{
							var Instance = new Items.Instance(Item.Type);
							Option<int[]> Slots = Plr.ItemGive(Instance);

							//TODO: Grab ungiven count from Instance to only pick up part of stack
							//Dropped items currently are only one item though

							Slots.MatchSome(
								(ActualSlots) =>
								{
									Plr.NotifyPickedUpItem();
									Net.SteelRpc(World.Self, nameof(World.RemoveDroppedItem), Item.Name);
									World.Self.RemoveDroppedItem(Item.Name);
								}
							);
						}
					);
				}
			}
		}

		if(!Possessed)
			return;

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
			Move(
				Momentum.Flattened()
					.Rotated(new Vector3(0,1,0), Deg2Rad(LoopRotation(-LookHorizontal)))
					.Rotated(new Vector3(1,0,0), Deg2Rad(LoopRotation(-ActualLookVertical)))
					.Rotated(new Vector3(0,1,0), Deg2Rad(LoopRotation(LookHorizontal))),
				Delta,
				1,
				60f,
				0f
			);

			Move(
				new Vector3(0, Momentum.y, 0)
					.Rotated(new Vector3(0,1,0), Deg2Rad(LoopRotation(LookHorizontal))),
				Delta,
				1,
				60f,
				0f
			);
		}
		else
			Momentum = Move(Momentum, Delta, 1, 60f, MovementSpeed);

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
			World.UnloadAndRequestChunks(Translation, Game.ChunkRenderDistance);
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
			LegsJointTarget = MaxAirLegRotation*SafeSign(ForwardMomentum);
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
	public void NetUpdateInventorySlot(int Slot, Items.ID ItemId, int Count)
	{
		if(Slot == 10)
		{
			//This is the eleventh slot, it is used only for dropping stacks
			if(Possessed)
			{
				Vector3 StartPos = Translation + Cam.Translation;
				for(int Index = 0; Index < Count; Index++)
					World.Self.DropItem(ItemId, StartPos, CalcThrowVelocity());
			}
		}
		else
			Inventory.UpdateSlot(Slot, ItemId, Count);

		if(Possessed)
			HUDInstance.HotbarUpdate();
		else if(Net.Work.IsNetworkServer())
			RpcId(Id, nameof(NetUpdateInventorySlot), Slot, ItemId, Count);
	}


	[Remote]
	public void NetEmptyInventorySlot(int Slot)
	{
		Inventory.EmptySlot(Slot);

		if(Possessed)
			HUDInstance.HotbarUpdate();
		else if(Net.Work.IsNetworkServer())
			RpcId(Id, nameof(NetEmptyInventorySlot), Slot);
	}


	[Remote]
	public void TransferTo(NodePath Path, int FromSlot, int ToSlot, Items.IntentCount CountMode)
	{
		if(!Net.Work.IsNetworkServer())
			RpcId(Net.ServerId, nameof(TransferTo), Path, FromSlot, ToSlot, CountMode);
		else
			Inventory.TransferTo(Path, FromSlot, ToSlot, CountMode);
	}


	public override void _Process(float Delta)
	{
		if(!Possessed)
		{
			NetUpdateDelta += Delta;
			return;
		}

		if(Dying)
			return;

		Assert.ActualAssert(MinAdsMultiplier > 0 && MinAdsMultiplier <= 1);
		AdsMultiplier =
			Ads
				? Clamp(AdsMultiplier - (Delta * (1 - MinAdsMultiplier) / AdsTime), MinAdsMultiplier, 1)
				: Clamp(AdsMultiplier + (Delta * (1 - MinAdsMultiplier) / AdsTime), MinAdsMultiplier, 1);

		Cam.Fov = Game.Fov*AdsMultiplier;

		float Length = ViewmodelMomentum.Length();
		Vector2 Normalized = ViewmodelMomentum.Normalized();
		ViewmodelMomentum = Normalized * Clamp(Length - Delta*3f, 0, float.MaxValue);

		ViewmodelItem.RotationDegrees = new Vector3(
			SafeSign(ViewmodelMomentum.y) * ViewmodelMomentum.y*ViewmodelMomentum.y * MaxViewmodelItemRotation * AdsMultiplier,
			180 - SafeSign(ViewmodelMomentum.x) * ViewmodelMomentum.x*ViewmodelMomentum.x * MaxViewmodelItemRotation * AdsMultiplier,
			0
		);
		ViewmodelArmJoint.RotationDegrees = new Vector3(
			SafeSign(ViewmodelMomentum.y) * ViewmodelMomentum.y*ViewmodelMomentum.y * MaxViewmodelArmRotation * AdsMultiplier,
			SafeSign(ViewmodelMomentum.x) * ViewmodelMomentum.x*ViewmodelMomentum.x * MaxViewmodelArmRotation * AdsMultiplier,
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


	public static Option<Player> None()
	{
		return Option.None<Player>();
	}


	public Option<Player> Some()
	{
		return Option.Some(this);
	}
}
