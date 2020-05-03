using Godot;
using static Godot.Mathf;
using System.Collections.Generic;



public class Hitscan : Spatial {
	public class AdditiveRecoil {
		public float Height = 0;
		public float Length = 0;
		public float Life = 0;

		public AdditiveRecoil(float HeightArg, float LengthArg) {
			Height = HeightArg;
			Length = LengthArg;

			Life = Length / 8; //We start the life partway through the curve
							   //The "correct" value would be Length/10 to start at the vertex
							   //Instead it starts *just* before the vertex
		}


		public float CaclulateOffset() {
			float DupStep = 5 * (Life / (Length / 2));
			return Pow(DupStep * E, 1 - DupStep) * Height;
		}
	}


	public class QueuedDamage {
		public int Id;
		public float Damage;
		public Vector3 Origin;

		public QueuedDamage(int IdArg, float DamageArg, Vector3 OriginArg) {
			Id = IdArg;
			Damage = DamageArg;
			Origin = OriginArg;
		}
	}


	public static float CrouchAffectPercentage = 0.6f; //Multiplied by recoil when crouching

	public static bool DebugDraw = false;

	public static float TrailStartAdjustment = -1;
	public static int NextRecoilDirection; //1 for right, -1 for left
	public static List<QueuedDamage> QueuedDamageList = new List<QueuedDamage>();

	private static PackedScene HitscanTrailScene = null;


	public static Hitscan Self;

	Hitscan() {
		if(Engine.EditorHint) { return; }

		HitscanTrailScene = GD.Load<PackedScene>("res://Items/Logic/Hitscan/HitscanTrail.tscn");

		Self = this;
		Reset();
	}


	public static void Reset() {
		NextRecoilDirection = 1;
	}


	public static void QueueFire(float VerticalAngle, float HorizontalAngle, float Range, float HDmg, float BDmg, float LDmg) {
		Game.PossessedPlayer.MatchSome(
			(Plr) => {
				PhysicsDirectSpaceState State = Self.GetWorld().DirectSpaceState;

				Vector3 Origin = Plr.Cam.GlobalTransform.origin;
				Vector3 Endpoint = Origin + new Vector3(0, 0, Range)
					.Rotated(new Vector3(1, 0, 0), Deg2Rad(-Plr.ActualLookVertical - VerticalAngle))
					.Rotated(new Vector3(0, 1, 0), Deg2Rad(Plr.LookHorizontal + HorizontalAngle));

				Godot.Collections.Dictionary Results = State.IntersectRay(Origin, Endpoint, null, 2);
				if(Results.Count > 0) //We hit something
				{
					Vector3 HitPoint = (Vector3)Results["position"];

					if(DebugDraw)
						World.DebugPlot(HitPoint);

					Self.DrawTrail(Origin + new Vector3(0, TrailStartAdjustment, 0), HitPoint);
					Net.SteelRpc(Self, nameof(DrawTrail), Origin + new Vector3(0, TrailStartAdjustment, 0), HitPoint);

					if(Results["collider"] is HitboxClass Hitbox) {
						Player HitPlr = Hitbox.OwningPlayer;

						float Damage = 0;
						switch(Hitbox.Type) {
							case HitboxClass.TYPE.HEAD:
								Damage = HDmg;
								break;
							case HitboxClass.TYPE.BODY:
								Damage = BDmg;
								break;
							case HitboxClass.TYPE.LEGS:
								Damage = LDmg;
								break;
						}


						bool UpdatedExisting = false;
						foreach(QueuedDamage Instance in QueuedDamageList) {
							if(Instance.Id == HitPlr.Id) {
								Instance.Damage += Damage;
								UpdatedExisting = true;
								break;
							}
						}
						if(!UpdatedExisting)
							QueuedDamageList.Add(new QueuedDamage(HitPlr.Id, Damage, Origin));
					}
				}
				else {
					Self.DrawTrail(Origin + new Vector3(0, TrailStartAdjustment, 0), Endpoint);
					Net.SteelRpc(Self, nameof(DrawTrail), Origin + new Vector3(0, TrailStartAdjustment, 0), Endpoint);
				}
			}
		);
	}


	public static void ApplyQueuedFire() {
		Game.PossessedPlayer.MatchSome(
			(Plr) => {
				if(QueuedDamageList.Count > 0)
					Plr.SfxManager.FpHitsound();

				foreach(QueuedDamage Instance in QueuedDamageList) {
					Net.Players[Instance.Id].Plr.MatchSome(
						(DamagedPlayer) => {
							if(DamagedPlayer.Health - Instance.Damage <= 0)
								Plr.SfxManager.FpKillsound();

							DamagedPlayer.Health -= Instance.Damage; //For high fire rate and high ping
							if(DamagedPlayer.Health < 0)
								DamagedPlayer.Health = 0;

							DamagedPlayer.RpcId(Instance.Id, nameof(Player.ApplyDamage), Instance.Damage, Instance.Origin);
						}
					);
				}

				QueuedDamageList.Clear();
			}
		);
	}


	public static void ApplyAdditiveRecoil(float Height, float Length) {
		Game.PossessedPlayer.MatchSome(
			(Plr) => {
				//Lessen recoil when ADS
				Height *= Plr.AdsMultiplier;
				Length *= Plr.AdsMultiplier;

				//Lessen recoil when crouching
				if(Plr.IsCrouching) {
					Height *= CrouchAffectPercentage;
					Length *= CrouchAffectPercentage;
				}

				Plr.ActiveAdditiveRecoil.Add(new AdditiveRecoil(Height, Length));
			}
		);
	}


	public static void ApplyEffectiveRecoil(float VerticalRecoil, float HorizontalRecoil) {
		Game.PossessedPlayer.MatchSome(
			(Plr) => {
				Assert.ActualAssert(NextRecoilDirection == 1 || NextRecoilDirection == -1);

				//Lessen recoil when ADS
				VerticalRecoil *= Plr.AdsMultiplier;
				HorizontalRecoil *= Plr.AdsMultiplier;

				//Lessen recoil when crouching
				if(Plr.IsCrouching) {
					VerticalRecoil *= CrouchAffectPercentage;
					HorizontalRecoil *= CrouchAffectPercentage;
				}

				Plr.ApplyLookVertical(VerticalRecoil);
				Plr.LookHorizontal += HorizontalRecoil * -NextRecoilDirection;
				Plr.RotationDegrees = new Vector3(0, Plr.LookHorizontal, 0);

				NextRecoilDirection = -NextRecoilDirection;
			}
		);
	}


	[Remote]
	public void DrawTrail(Vector3 Start, Vector3 End) //Must be non-static to be RPC-ed
	{
		var Trail = (HitscanTrail)HitscanTrailScene.Instance();
		World.EntitiesRoot.AddChild(Trail);
		Trail.Translation = (Start + End) / 2;

		Trail.LookAt(End, -End);

		Trail.CallDeferred(nameof(HitscanTrail.ApplyLength), Start.DistanceTo(End));
	}
}
