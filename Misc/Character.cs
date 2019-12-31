using System;
using Godot;
using static Godot.Mathf;



public class Character : KinematicBody
{
	public bool OnFloor { get; private set; } = false;


	public Vector3 Move(Vector3 Momentum, float Delta, int MaxSlideCount, float MaxAngle, float Snap)
	{
		Vector3 Movement = Momentum * Delta;

		if(Momentum.y <= 0)
		{
			Vector3 OriginalTranslation = Translation;
			var SnapVec = new Vector3(0, (-Snap - 0.1f) * Delta, 0);
			KinematicCollision SnapCollision = MoveAndCollide(SnapVec);
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
					Momentum.y = 0; //On floor so zero out vertical momentum
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

		if(OnFloor)
			Momentum.y = 0; //On floor so zero out vertical momentum
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
