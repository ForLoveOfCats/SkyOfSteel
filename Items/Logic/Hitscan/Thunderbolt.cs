using Godot;


public static class Thunderbolt
{
	public static float VerticalRecoil = 22;
	public static float RecoilLength = 3;
	public static float Range = 500;
	public static float HeadshotDamage = 100;
	public static float BodyshotDamage = 35;
	public static float LegshotDamage = 20;
	public static float FireCooldown = 50;


	public static void Fire(Items.Instance Item, Player UsingPlayer)
	{
		Hitscan.Fire(0, 0, Range, HeadshotDamage, BodyshotDamage, LegshotDamage);
		Hitscan.ApplyAdditiveRecoil(VerticalRecoil, RecoilLength);

		UsingPlayer.SetCooldown(0, FireCooldown, true);
		UsingPlayer.SfxManager.FpThunderboltFire();
	}
}
