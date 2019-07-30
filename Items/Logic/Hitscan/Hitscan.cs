using Godot;
using static Godot.Mathf;
using static System.Diagnostics.Debug;


public class Hitscan : Spatial
{
	public static bool DebugDraw = false;

	public static float TrailStartAdjustment = 1;
	public static int NextRecoilDirection; //1 for right, -1 for left

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


	public static void Fire(float VerticalAngle, float HorizontalAngle, float Range, float HDmg, float BDmg, float LDmg)
	{
		Assert(NextRecoilDirection == 1 || NextRecoilDirection == -1);
		Player Plr = Game.PossessedPlayer;

		{
			PhysicsDirectSpaceState State = Self.GetWorld().DirectSpaceState;

			Vector3 Origin = Plr.Cam.GlobalTransform.origin;
			Vector3 Endpoint = Origin + new Vector3(0, 0, Range)
				.Rotated(new Vector3(1, 0, 0), Deg2Rad(-Plr.LookVertical - VerticalAngle))
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
					Game.PossessedPlayer.SfxManager.FpHitsound();

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

					if(HitPlr.Health - Damage <= 0)
						Game.PossessedPlayer.SfxManager.FpKillsound();

					HitPlr.RpcId(HitPlr.Id, nameof(Player.ApplyDamage), Damage, Game.PossessedPlayer.Id);
				}
			}
			else
			{
				Self.DrawTrail(Origin, Endpoint);
				Net.SteelRpc(Self, nameof(DrawTrail), Origin, Endpoint);
			}
		}
	}


	public static void ApplyRecoil(float VerticalRecoil, float HorizontalRecoil)
	{
		Player Plr = Game.PossessedPlayer;
		Plr.ApplyLookVertical(VerticalRecoil);
		Plr.LookHorizontal -= HorizontalRecoil*NextRecoilDirection;
		Plr.SetRotationDegrees(new Vector3(0, Plr.LookHorizontal, 0));

		NextRecoilDirection *= -1;
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
