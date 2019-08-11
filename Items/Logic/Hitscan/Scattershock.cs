using Godot;


public static class Scattershock
{
	public static float VerticalRecoil = 36;
	public static float RecoilLength = 6f;
	public static float Range = 500;
	public static float AngularOffset = 1.5f;
	public static float HeadshotDamage = 10;
	public static float BodyshotDamage = 3;
	public static float LegshotDamage = 2;
	public static float FireCooldown = 25;


	public static void Fire(Items.Instance Item, Player UsingPlayer)
	{
		{
			float Multiplyer = 1;
			if(Game.PossessedPlayer.Ads)
				Multiplyer = Game.PossessedPlayer.AdsMultiplyer;

			for(int x = -2; x <= 2; x++)
			{
				for(int y = -2; y <= 2; y++)
				{
					Hitscan.QueueFire(x*AngularOffset*Multiplyer, y*AngularOffset*Multiplyer, Range, HeadshotDamage, BodyshotDamage, LegshotDamage);
				}
			}

			Hitscan.ApplyQueuedFire();
		}

		Hitscan.ApplyAdditiveRecoil(VerticalRecoil, RecoilLength);

		UsingPlayer.SetCooldown(0, FireCooldown*UsingPlayer.AdsMultiplyer, true);
		UsingPlayer.SfxManager.FpScattershockFire();
	}
}
