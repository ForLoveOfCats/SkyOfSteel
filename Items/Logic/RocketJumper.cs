using Godot;
using static Godot.Mathf;
using static SteelMath;


public class RocketJumper : Node
{
	public static float RocketTravelSpeed = 120; //Units-per-second
	public static float RocketFuseTime = 4f; //In seconds
	public static float MaxRocketPush = 70; //Units-per-second force applied
	public static float MaxRocketDistance = 30; //Make sure that radius of AffectArea on JumperRocket matches
	public static float MinRocketDistance = 8;
	public static float RocketHorizontalMultiplyer = 1.1f;
	public static float RocketVerticalDivisor = 1.7f;

	public static PackedScene JumperRocketScene;

	public static RocketJumper Self;

	RocketJumper()
	{
		Self = this;
	}

	static RocketJumper()
	{
		JumperRocketScene = GD.Load<PackedScene>("Items/Logic/JumperRocket.tscn");
	}


	public static void Fire(Items.Instance Item, Player UsingPlayer)
	{
		JumperRocket Rocket = JumperRocketScene.Instance() as JumperRocket;
		Rocket.IsLocal = true;
		Rocket.Player = UsingPlayer;
		Rocket.Translation = UsingPlayer.RocketStart.GetGlobalTransform().origin;
		Rocket.RotationDegrees = new Vector3(-UsingPlayer.LookVertical, UsingPlayer.LookHorizontal, 0);
		Rocket.Momentum = new Vector3(0, 0, RocketTravelSpeed)
			.Rotated(new Vector3(1,0,0), Deg2Rad(Rocket.RotationDegrees.x))
			.Rotated(new Vector3(0,1,0), Deg2Rad(Rocket.RotationDegrees.y));
		Rocket.Name = System.Guid.NewGuid().ToString();
		World.EntitiesRoot.AddChild(Rocket);

		Net.SteelRpc(Self, nameof(RemoteFire), Rocket.Translation, Rocket.RotationDegrees, Rocket.Momentum, Rocket.GetName());
	}


	[Remote]
	public void RemoteFire(Vector3 Position, Vector3 Rotation, Vector3 Momentum, string Name)
	{
		JumperRocket Rocket = JumperRocketScene.Instance() as JumperRocket;
		Rocket.IsLocal = false;
		Rocket.Translation = Position;
		Rocket.RotationDegrees = Rotation;
		Rocket.Momentum = Momentum;
		Rocket.Name = Name;
		World.EntitiesRoot.AddChild(Rocket);
	}
}
