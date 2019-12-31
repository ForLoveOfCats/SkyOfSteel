using System;
using Godot;
using static Godot.Mathf;



public class Character : KinematicBody
{
	public bool OnFloor { get; private set; } = false;


	public Vector3 Move(Vector3 Momentum, float Delta, int MaxSlideCount, float MaxAngle)
	{
		Vector3 Movement = Momentum * Delta;

		if(Momentum.y <= 0)
		{
			Vector3 OriginalTranslation = Translation;
			var Snap = new Vector3(0, -Movement.Flattened().Length() - 0.2f, 0);
			KinematicCollision SnapCollision = MoveAndCollide(Snap);
			if(SnapCollision != null && Acos(SnapCollision.Normal.Dot(new Vector3(0, 1, 0))) <= Deg2Rad(MaxAngle))
			{
				float TargetHLength = Movement.Flattened().Length();
				Movement = Movement.Slide(SnapCollision.Normal);
				float NewHLength = Movement.Flattened().Length();
				if(TargetHLength != 0 && NewHLength != 0) //Protect against division by zero
					Movement *= TargetHLength / Movement.Flattened().Length();

				OnFloor = true;

				if(Momentum.Flattened().Length() <= 0.5f)
				{
					Translation = OriginalTranslation;
					return Momentum;
				}
			}
			else
			{
				Translation = OriginalTranslation;
				OnFloor = false;
			}
		}
		else
			OnFloor = false;

		int SlideCount = 0;
		float Traveled = 0f;
		while(SlideCount <= MaxSlideCount)
		{
			Movement = Movement.Normalized() * (Movement.Length() - Traveled);
			KinematicCollision Collision = MoveAndCollide(Movement);
			if(Collision == null)
				break; //No collision, reached destination

			Traveled += Collision.Travel.Length();
			if(Traveled >= Movement.Length())
				break; //Reached destination
			SlideCount += 1;

			Movement = Movement.Slide(Collision.Normal);
			if(Acos(Collision.Normal.Dot(new Vector3(0, 1, 0))) <= Deg2Rad(MaxAngle))
				OnFloor = true;
			else
				Momentum = Momentum.Slide(Collision.Normal);
		}

		return Momentum;
	}


	[Obsolete] public new bool IsOnFloor()
	{
		return OnFloor;
	}


	[Obsolete] public new bool IsOnWall()
	{
		return base.IsOnWall();
	}


	[Obsolete] public new bool IsOnCeiling()
	{
		return base.IsOnCeiling();
	}
}
