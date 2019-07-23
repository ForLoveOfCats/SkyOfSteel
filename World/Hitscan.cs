using Godot;
using static System.Diagnostics.Debug;


public class Hitscan : Spatial
{
	public static int NextRecoilDirection; //1 for right, -1 for left


	public static Hitscan Self;

	Hitscan()
	{
		if(Engine.EditorHint) {return;}

		Self = this;
		Reset();
	}


	public static void Reset()
	{
		NextRecoilDirection = 1;
	}


	public static void Fire(float VerticalRecoil, float HorizontalRecoil)
	{
		GD.Print("Hitscan.Fire");

		Assert(NextRecoilDirection == 1 || NextRecoilDirection == -1);

		Player Plr = Game.PossessedPlayer;
		Plr.ApplyLookVertical(VerticalRecoil);
		Plr.LookHorizontal -= HorizontalRecoil*NextRecoilDirection;
		Plr.SetRotationDegrees(new Vector3(0, Plr.LookHorizontal, 0));

		NextRecoilDirection *= -1;
	}
}
