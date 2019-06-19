using Godot;
using static Godot.Mathf;
using static SteelMath;


public class RocketJumper
{
	public static float MaxRocketPush = 80;
	public static float MaxRocketDistance = 30;
	public static float RocketHorizontalMultiplyer = 1.1f;
	public static float RocketVerticalDivisor = 1.7f;


	public static void Fire(Items.Instance Item, Player UsingPlayer)
	{
		RayCast Cast = UsingPlayer.GetNode<RayCast>("SteelCamera/RocketRayCast");
		if(Cast.IsColliding())
		{
			float Distance = UsingPlayer.Translation.DistanceTo(Cast.GetCollisionPoint());
			float Power = (MaxRocketPush/MaxRocketDistance) * Clamp(MaxRocketDistance - Distance, 0, MaxRocketDistance);

			Camera Cam = UsingPlayer.GetNode<Camera>("SteelCamera");
			Vector3 Push = new Vector3(0, 0, Power);
			Push = Push.Rotated(new Vector3(1,0,0), Deg2Rad(LoopRotation(Cam.RotationDegrees.x)));
			Push = Push.Rotated(new Vector3(0,1,0), Deg2Rad(LoopRotation(UsingPlayer.RotationDegrees.y + 180)));

			{
				Vector3 Flat = Push;
				Flat.y = 0;
				Flat *= RocketHorizontalMultiplyer;
				Push.x = Flat.x;
				Push.z = Flat.z;
			}
			Push.y /= RocketVerticalDivisor;

			UsingPlayer.ApplyPush(Push);
			UsingPlayer.RecoverPercentage = 0;
		}
	}
}
