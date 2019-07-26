using Godot;


public class Thunderbolt : Node
{
	public static float VerticalRecoil = 8;
	public static float HorizontalRecoil = 3;
	public static float Range = 500;
	public static float FireCooldown = 40;

	public static Thunderbolt Self;

	Thunderbolt()
	{
		if(Engine.EditorHint) {return;}

		Self = this;
	}


	public static void Fire(Items.Instance Item, Player UsingPlayer)
	{
		Hitscan.Fire(0, 0, Range);
		Hitscan.ApplyRecoil(VerticalRecoil, HorizontalRecoil);

		UsingPlayer.SetCooldown(0, FireCooldown, true);
		UsingPlayer.SfxManager.FpThunderboltFire();
	}
}
