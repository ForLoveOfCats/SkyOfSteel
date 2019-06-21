using Godot;
using static Godot.Mathf;
using static SteelMath;


public class RocketJumper
{
	public static float RocketTravelSpeed = 120; //Units-per-second
	public static float RocketArmTime = 0.05f; //In seconds
	public static float RocketFuseTime = 4f; //In seconds
	public static float MaxRocketPush = 80; //Units-per-second force applied
	public static float MaxRocketDistance = 30; //Make sure that radius of AffectArea on JumperRocket matches
	public static float RocketHorizontalMultiplyer = 1.1f;
	public static float RocketVerticalDivisor = 1.7f;

	public static PackedScene JumperRocketScene;

	static RocketJumper()
	{
		JumperRocketScene = GD.Load<PackedScene>("Items/Logic/JumperRocket.tscn");
	}


	public static void Fire(Items.Instance Item, Player UsingPlayer)
	{
		JumperRocket Rocket = JumperRocketScene.Instance() as JumperRocket;
		Rocket.Player = UsingPlayer;
		Rocket.Translation = UsingPlayer.Translation + UsingPlayer.Cam.Translation;
		Rocket.RotationDegrees = new Vector3(-UsingPlayer.LookVertical, UsingPlayer.LookHorizontal, 0);
		Rocket.Momentum = new Vector3(0, 0, RocketTravelSpeed)
			.Rotated(new Vector3(1,0,0), Deg2Rad(Rocket.RotationDegrees.x))
			.Rotated(new Vector3(0,1,0), Deg2Rad(Rocket.RotationDegrees.y))
			+ UsingPlayer.Momentum;
		World.EntitiesRoot.AddChild(Rocket);
	}
}
