using Godot;


public class Thunderbolt : Node
{
	public static float VerticalRecoil = 8;
	public static float HorizontalRecoil = 2;
	public static float Range = 500;
	public static float HeadshotDamage = 100;
	public static float BodyshotDamage = 20;
	public static float LegshotDamage = 10;
	public static float FireCooldown = 50;

	public static Thunderbolt Self;

	Thunderbolt()
	{
		if(Engine.EditorHint) {return;}

		Self = this;
	}


	public static void Fire(Items.Instance Item, Player UsingPlayer)
	{
		Hitscan.Fire(0, 0, Range, HeadshotDamage, BodyshotDamage, LegshotDamage);
		Hitscan.ApplyRecoil(VerticalRecoil, HorizontalRecoil);

		UsingPlayer.SetCooldown(0, FireCooldown, true);
		UsingPlayer.SfxManager.FpThunderboltFire();
	}
}
