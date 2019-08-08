using Godot;


public static class Scattershock
{
	public static float VerticalRecoil = 22;
	public static float RecoilLength = 4;
	public static float Range = 500;
	public static float AngularOffset = 2f;
	public static float HeadshotDamage = 30;
	public static float BodyshotDamage = 25;
	public static float LegshotDamage = 10;
	public static float FireCooldown = 55;

	public static void Fire(Items.Instance Item, Player UsingPlayer)
	{
		{
			float Multiplyer = 1;
			if(Game.PossessedPlayer.Ads)
				Multiplyer = Game.PossessedPlayer.AdsMultiplyer;

			for(int x = -1; x <= 1; x++)
			{
				for(int y = -1; y <= 1; y++)
				{
					Hitscan.Fire(x*AngularOffset*Multiplyer, y*AngularOffset*Multiplyer, Range, HeadshotDamage, BodyshotDamage, LegshotDamage);
				}
			}
		}

		Hitscan.ApplyAdditiveRecoil(VerticalRecoil, RecoilLength);

		UsingPlayer.SetCooldown(0, FireCooldown, true);
		UsingPlayer.SfxManager.FpThunderboltFire(); //TODO: Add scattershock fire sfx
	}
}
