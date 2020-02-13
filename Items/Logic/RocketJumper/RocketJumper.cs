using Godot;
using static Godot.Mathf;



public class RocketJumper : Node
{
	public static float FireCooldown = 40;


	public static void Fire(Items.Instance Item, Player UsingPlayer)
	{
		Projectiles.Fire(Projectiles.ProjectileID.ROCKET_JUMPER, UsingPlayer);

		UsingPlayer.SfxManager.FpRocketFire();
		UsingPlayer.SetCooldown(0, FireCooldown, true);
	}
}
