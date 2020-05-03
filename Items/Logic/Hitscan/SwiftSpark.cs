using Godot;


public static class SwiftSpark {
	public static float VerticalRecoil = 2;
	public static float HorizontalRecoil = 1;
	public static float Range = 400;
	public static float HeadshotDamage = 25;
	public static float BodyshotDamage = 20;
	public static float LegshotDamage = 15;
	public static float FireCooldown = 10;


	public static void Fire(Items.Instance Item, Player UsingPlayer) {
		Hitscan.QueueFire(0, 0, Range, HeadshotDamage, BodyshotDamage, LegshotDamage);
		Hitscan.ApplyQueuedFire();

		Hitscan.ApplyEffectiveRecoil(VerticalRecoil, HorizontalRecoil);
		UsingPlayer.SetCooldown(0, FireCooldown, true);

		UsingPlayer.SfxManager.FpThunderboltFire();
	}
}
