using Godot;
using static Godot.Mathf;
using System.Collections.Generic;
using static System.Diagnostics.Debug;


public class Hitscan : Spatial
{
	public class AdditiveRecoil
	{
		public float Height = 0;
		public float Length = 0;
		public float Life = 0;

		public AdditiveRecoil(float HeightArg, float LengthArg)
		{
			Height = HeightArg;
			Length = LengthArg;

			Life = Length/8; //We start the life partway through the curve
			//The "correct" value would be Length/10 to start at the vertex
			//Instead it starts *just* before the vertex
		}


		public float CaclulateOffset()
		{
			float DupStep = 5*(Life/(Length/2));
			return Pow(DupStep*E, 1-DupStep) * Height;
		}
	}


	public class QueuedDamage
	{
		public int Id;
		public float Damage;
		public Vector3 Origin;

		public QueuedDamage(int IdArg, float DamageArg, Vector3 OriginArg)
		{
			Id = IdArg;
			Damage = DamageArg;
			Origin = OriginArg;
		}
	}


	public static bool DebugDraw = false;

	public static float TrailStartAdjustment = 1;
	public static int NextRecoilDirection; //1 for right, -1 for left
	public static List<QueuedDamage> QueuedDamageList = new List<QueuedDamage>();

	private static PackedScene HitscanTrailScene = null;


	public static Hitscan Self;

	Hitscan()
	{
		if(Engine.EditorHint) {return;}

		HitscanTrailScene = GD.Load<PackedScene>("res://Items/Logic/Hitscan/HitscanTrail.tscn");

		Self = this;
		Reset();
	}


	public static void Reset()
	{
		NextRecoilDirection = 1;
	}


	public static void QueueFire(float VerticalAngle, float HorizontalAngle, float Range, float HDmg, float BDmg, float LDmg)
	{
		Assert(NextRecoilDirection == 1 || NextRecoilDirection == -1);
		Player Plr = Game.PossessedPlayer;

		{
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

				Self.DrawTrail(Origin, HitPoint);
				Net.SteelRpc(Self, nameof(DrawTrail), Origin, HitPoint);

				if(Results["collider"] is HitboxClass Hitbox)
				{
					Player HitPlr = Hitbox.OwningPlayer;

					float Damage = 0;
					switch(Hitbox.Type)
					{
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
					foreach(QueuedDamage Instance in QueuedDamageList)
					{
						if(Instance.Id == HitPlr.Id)
						{
							Instance.Damage += Damage;
							UpdatedExisting = true;
							break;
						}
					}
					if(!UpdatedExisting)
						QueuedDamageList.Add(new QueuedDamage(HitPlr.Id, Damage, Origin));
				}
			}
			else
			{
				Self.DrawTrail(Origin, Endpoint);
				Net.SteelRpc(Self, nameof(DrawTrail), Origin, Endpoint);
			}
		}
	}


	public static void ApplyQueuedFire()
	{
		if(QueuedDamageList.Count > 0)
			Game.PossessedPlayer.SfxManager.FpHitsound();

		foreach(QueuedDamage Instance in QueuedDamageList)
		{
			Player DamagedPlayer = Net.Players[Instance.Id];

			if(DamagedPlayer.Health - Instance.Damage <= 0)
				Game.PossessedPlayer.SfxManager.FpKillsound();

			DamagedPlayer.RpcId(Instance.Id, nameof(Player.ApplyDamage), Instance.Damage, Instance.Origin);
		}

		QueuedDamageList.Clear();
	}


	public static void ApplyAdditiveRecoil(float Height, float Length)
	{
		//Lessen recoil when ADS
		Height *= Game.PossessedPlayer.AdsMultiplyer;
		Length *= Game.PossessedPlayer.AdsMultiplyer;

		Game.PossessedPlayer.ActiveAdditiveRecoil.Add(new AdditiveRecoil(Height, Length));
	}


	[Remote]
	public void DrawTrail(Vector3 Start, Vector3 End) //Must be non-static to be RPC-ed
	{
		Start.y -= TrailStartAdjustment;
		HitscanTrail Trail = HitscanTrailScene.Instance() as HitscanTrail;
		World.EntitiesRoot.AddChild(Trail);
		Trail.Translation = (Start + End) / 2;

		Trail.LookAt(End, -End);

		Trail.CallDeferred(nameof(HitscanTrail.ApplyLength), Start.DistanceTo(End));
	}
}
