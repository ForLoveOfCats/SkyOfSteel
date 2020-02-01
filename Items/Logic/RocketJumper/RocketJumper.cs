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
		Projectiles.Fire(Projectiles.ProjectileID.ROCKET_JUMPER, UsingPlayer);

		UsingPlayer.SfxManager.FpRocketFire();
		UsingPlayer.SetCooldown(0, FireCooldown, true);
	}
}
