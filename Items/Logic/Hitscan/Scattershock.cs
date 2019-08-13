using static Godot.Mathf;


public static class Scattershock
{
	public static float VerticalRecoil = 36;
	public static float RecoilLength = 6f;
	public static float Range = 500;
	public static float AngularOffset = 2.8f;
	public static float HeadshotDamage = 3.8f;
	public static float BodyshotDamage = 2.5f;
	public static float LegshotDamage = 1.5f;
	public static float FireCooldown = 25;


	public static void Fire(Items.Instance Item, Player UsingPlayer)
	{
		{
			float Multiplyer = Pow(UsingPlayer.AdsMultiplyer, 2);

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
