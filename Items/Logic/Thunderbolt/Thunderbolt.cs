using Godot;


public class Thunderbolt : Node
{
	public static float VerticalRecoil = 15;
	public static float HorizontalRecoil = 8;

	public static Thunderbolt Self;

	Thunderbolt()
	{
		if(Engine.EditorHint) {return;}

		Self = this;
	}


	public static void Fire(Items.Instance Item, Player UsingPlayer)
	{
		Hitscan.Fire(VerticalRecoil, HorizontalRecoil);
	}
}
