using Godot;
using static Godot.Mathf;



public class RocketJumper : Node
{
	public static float RocketTravelSpeed = 150; //Units-per-second
	public static float RocketFuseTime = 4f; //In seconds
	public static float MaxRocketPush = 72; //Units-per-second force applied
	public static float MaxRocketDistance = 30; //Make sure that radius of AffectArea on JumperRocket matches
	public static float RocketHorizontalMultiplyer = 1f;
	public static float RocketVerticalMultiplyer = 0.65f;
	public static float FireCooldown = 40;

	public static PackedScene JumperRocketScene;

	public static RocketJumper Self;

	RocketJumper()
	{
		if(Engine.EditorHint) {return;}

		Self = this;
	}

	static RocketJumper()
	{
		JumperRocketScene = GD.Load<PackedScene>("Items/Logic/RocketJumper/JumperRocket.tscn");
	}


	public static void Fire(Items.Instance Item, Player UsingPlayer)
	{
		var Rocket = (JumperRocket) JumperRocketScene.Instance();
		Rocket.FirerId = UsingPlayer.Id;
		Rocket.Translation = UsingPlayer.ProjectileEmitter.GlobalTransform.origin;
		Rocket.RotationDegrees = new Vector3(-UsingPlayer.IntendedLookVertical, UsingPlayer.LookHorizontal, 0);
		Rocket.Momentum = new Vector3(0, 0, RocketTravelSpeed)
			.Rotated(new Vector3(1,0,0), Deg2Rad(Rocket.RotationDegrees.x))
			.Rotated(new Vector3(0,1,0), Deg2Rad(Rocket.RotationDegrees.y));
		Rocket.Name = System.Guid.NewGuid().ToString();
		World.EntitiesRoot.AddChild(Rocket);

		Net.SteelRpc(Self, nameof(RemoteFire), UsingPlayer.Id, Rocket.Translation, Rocket.RotationDegrees, Rocket.Momentum, Rocket.Name);

		UsingPlayer.SfxManager.FpRocketFire();
		UsingPlayer.SetCooldown(0, FireCooldown, true);
	}


	[Remote]
	public void RemoteFire(int Firer, Vector3 Position, Vector3 Rotation, Vector3 Momentum, string Name)
	{
		var Rocket = (JumperRocket) JumperRocketScene.Instance();
		Rocket.FirerId = Firer;
		Rocket.Translation = Position;
		Rocket.RotationDegrees = Rotation;
		Rocket.Momentum = Momentum;
		Rocket.Name = Name;
		World.EntitiesRoot.AddChild(Rocket);
	}
}
