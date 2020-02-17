using Godot;
using static Godot.Mathf;



public class PlayerInput
{
	[SteelInputWithoutArg(typeof(PlayerInput), nameof(ToggleFly))]
	public static void ToggleFly()
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) => Plr.SetFly(!Plr.FlyMode)
		);
	}


	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InputReset))]
	public static void InputReset()
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) => Plr.Reset()
		);
	}


	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InventoryUp))]
	public static void InventoryUp()
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				if(!(Plr.CurrentCooldown < Plr.CurrentMaxCooldown && Plr.PreventSwitch))
				{
					Plr.BuildRotation = 0;

					Plr.InventorySlot--;
					if(Plr.InventorySlot < 0)
						Plr.InventorySlot = 9;

					Plr.HUDInstance.HotbarUpdate();
					Hitscan.Reset();
					Plr.SetCooldown(0, Player.SlotSwitchCooldown, false);
					Plr.Ads = false;
				}
			}
		);
	}


	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InventoryDown))]
	public static void InventoryDown()
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				if(!(Plr.CurrentCooldown < Plr.CurrentMaxCooldown && Plr.PreventSwitch))
				{
					Plr.BuildRotation = 0;

					Plr.InventorySlot++;
					if(Plr.InventorySlot > 9)
						Plr.InventorySlot = 0;

					Plr.HUDInstance.HotbarUpdate();
					Hitscan.Reset();
					Plr.SetCooldown(0, Player.SlotSwitchCooldown, false);
					Plr.Ads = false;
				}
			}
		);
	}


	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InventorySlot0))]
	public static void InventorySlot0() => Game.PossessedPlayer.MatchSome( (Plr) => Plr.InventorySlotSelect(0) );

	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InventorySlot1))]
	public static void InventorySlot1() => Game.PossessedPlayer.MatchSome( (Plr) => Plr.InventorySlotSelect(1) );

	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InventorySlot2))]
	public static void InventorySlot2() => Game.PossessedPlayer.MatchSome( (Plr) => Plr.InventorySlotSelect(2) );

	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InventorySlot3))]
	public static void InventorySlot3() => Game.PossessedPlayer.MatchSome( (Plr) => Plr.InventorySlotSelect(3) );

	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InventorySlot4))]
	public static void InventorySlot4() => Game.PossessedPlayer.MatchSome( (Plr) => Plr.InventorySlotSelect(4) );

	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InventorySlot5))]
	public static void InventorySlot5() => Game.PossessedPlayer.MatchSome( (Plr) => Plr.InventorySlotSelect(5) );

	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InventorySlot6))]
	public static void InventorySlot6() => Game.PossessedPlayer.MatchSome( (Plr) => Plr.InventorySlotSelect(6) );

	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InventorySlot7))]
	public static void InventorySlot7() => Game.PossessedPlayer.MatchSome( (Plr) => Plr.InventorySlotSelect(7) );

	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InventorySlot8))]
	public static void InventorySlot8() => Game.PossessedPlayer.MatchSome( (Plr) => Plr.InventorySlotSelect(8) );

	[SteelInputWithoutArg(typeof(PlayerInput), nameof(InventorySlot9))]
	public static void InventorySlot9() => Game.PossessedPlayer.MatchSome( (Plr) => Plr.InventorySlotSelect(9) );


	[SteelInputWithArg(typeof(PlayerInput), nameof(BuildRotate))]
	public static void BuildRotate(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				if(Sens > 0 && Plr.Inventory[Plr.InventorySlot] != null)
				{
					Plr.BuildRotation++;
					if(Plr.BuildRotation > 3)
						Plr.BuildRotation = 0;
				}
			}
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(ForwardMove))]
	public static void ForwardMove(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
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
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(BackwardMove))]
	public static void BackwardMove(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
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
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(RightMove))]
	public static void RightMove(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
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
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(LeftMove))]
	public static void LeftMove(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				Plr.LeftSens = Sens;
				if(Sens > 0)
				{
					Plr.RightAxis = -1;
				}
				else if(Plr.RightAxis < 0)
				{
					Plr.RightAxis = 0;
					if(Plr.RightSens > 0)
					{
						Plr.RightAxis = 1;
					}
				}
			}
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(FlySprint))]
	public static void FlySprint(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				Plr.FlySprintSens = Sens;
				if(Sens > 0 && Plr.FlyMode)
				{
					Plr.IsFlySprinting = true;

					if(Plr.JumpAxis == 1)
						Plr.Momentum.y = Player.MovementSpeed * Player.FlySprintMultiplier;
					else if(Plr.IsCrouching)
						Plr.Momentum.y = -Player.MovementSpeed * Player.FlySprintMultiplier;
				}
				else
				{
					Plr.IsFlySprinting = false;
					Plr.Momentum.y = Clamp(Plr.Momentum.y, -Player.MovementSpeed, Player.MovementSpeed);
				}
			}
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(Jump))]
	public static void Jump(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				Plr.JumpSens = Sens;
				if(Sens > 0)
				{
					if(Plr.FlyMode)
					{
						if(Plr.IsFlySprinting)
						{
							Plr.Momentum.y = Player.MovementSpeed * Player.FlySprintMultiplier;
						}
						else
						{
							Plr.Momentum.y = Player.MovementSpeed;
						}

						Plr.IsJumping = false;
					}
					else if(Plr.OnFloor)
					{
						Plr.Momentum.y = Player.JumpStartForce;
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
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(Crouch))]
	public static void Crouch(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
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
							Plr.Momentum.y = -Player.MovementSpeed * Player.FlySprintMultiplier;
						else
							Plr.Momentum.y = -Player.MovementSpeed;
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
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(LookUp))]
	public static void LookUp(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				if(Sens > 0)
				{
					float Change = ((float) Sens / Player.LookDivisor) * Game.LookSensitivity * Plr.AdsMultiplier;

					Plr.ApplyLookVertical(Change);

					Plr.ViewmodelMomentum = new Vector2(
						Plr.ViewmodelMomentum.x,
						Clamp(Plr.ViewmodelMomentum.y + Sens*Player.ViewmodelSensitivity*Plr.AdsMultiplier, -1, 1)
					);
				}
			}
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(LookDown))]
	public static void LookDown(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				if(Sens > 0)
				{
					float Change = ((float) Sens / Player.LookDivisor) * Game.LookSensitivity * Plr.AdsMultiplier;

					Plr.ApplyLookVertical(-Change);

					Plr.ViewmodelMomentum = new Vector2(
						Plr.ViewmodelMomentum.x,
						Clamp(Plr.ViewmodelMomentum.y - Sens*Player.ViewmodelSensitivity*Plr.AdsMultiplier, -1, 1)
					);
				}
			}
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(LookRight))]
	public static void LookRight(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				if(Sens > 0)
				{
					float Change = ((float) Sens / Player.LookDivisor) * Game.LookSensitivity * Plr.AdsMultiplier;

					Plr.LookHorizontal -= Change;
					Plr.RotationDegrees = new Vector3(0, Plr.LookHorizontal, 0);

					Plr.ViewmodelMomentum = new Vector2(
						Clamp(Plr.ViewmodelMomentum.x - Sens*Player.ViewmodelSensitivity*Plr.AdsMultiplier, -1, 1),
						Plr.ViewmodelMomentum.y
					);
				}
			}
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(LookLeft))]
	public static void LookLeft(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				if(Sens > 0)
				{
					float Change = ((float) Sens / Player.LookDivisor) * Game.LookSensitivity * Plr.AdsMultiplier;

					Plr.LookHorizontal += Change;
					Plr.RotationDegrees = new Vector3(0, Plr.LookHorizontal, 0);

					Plr.ViewmodelMomentum = new Vector2(
						Clamp(Plr.ViewmodelMomentum.x + Sens*Player.ViewmodelSensitivity*Plr.AdsMultiplier, -1, 1),
						Plr.ViewmodelMomentum.y
					);
				}
			}
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(PrimaryFire))]
	public static void PrimaryFire(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
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
										Plr.SetCooldown(0, Player.BuildingCooldown, true);
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
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(SecondaryFire))]
	public static void SecondaryFire(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
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
									Plr.SetCooldown(0, Player.BuildingCooldown, true);
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
		);
	}


	[SteelInputWithoutArg(typeof(PlayerInput), nameof(Interact))]
	public static void Interact()
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				Vector3 Start = Plr.Translation;
				Vector3 End = Start + new Vector3(0, 0, Player.InteractReach)
					.Rotated(new Vector3(1, 0, 0), Deg2Rad(Plr.ActualLookVertical))
					.Rotated(new Vector3(0, 1, 0), Deg2Rad(Plr.LookHorizontal));
				var Exclude = new Godot.Collections.Array {Plr};

				PhysicsDirectSpaceState State = Plr.GetWorld().DirectSpaceState;
				Godot.Collections.Dictionary Results = State.IntersectRay(Start, End, Exclude, 4);
				if(Results.Count > 0)
				{
					object RawCollider = Results["collider"];
					if(RawCollider is Locker CollidedLocker)
						Menu.BuildInteractInventory(CollidedLocker);
				}
			}
		);
	}


	[SteelInputWithArg(typeof(PlayerInput), nameof(ThrowCurrentItem))]
	public static void ThrowCurrentItem(float Sens)
	{
		Game.PossessedPlayer.MatchSome(
			(Plr) =>
			{
				if(Sens > 0)
				{
					Vector3 Vel = Plr.CalcThrowVelocity();

					if(Net.Work.IsNetworkServer())
						Plr.ThrowItemFromSlot(Plr.InventorySlot, Vel);
					else
					{
						Plr.RpcId(Net.ServerId, nameof(Player.ThrowItemFromSlot), Plr.InventorySlot, Vel);

						if(Plr.Inventory[Plr.InventorySlot] != null)
						{
							Plr.SfxManager.FpThrow();
							Plr.SetCooldown(0, Player.SlotSwitchCooldown, false);
						}
					}
				}
			}
		);
	}
}
