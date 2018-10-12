using Godot;
using System;


public class SMath : Node
{
	private const float MaximumDifferenceSimilar = 0.5f;

	public static SMath Self;
	SMath()
	{
		Self = this;
	}


	public static bool AreVectorsSimilar(Vector3 Vec1, Vector3 Vec2)
	{
		if(Vec1.DistanceSquaredTo(Vec2) > MaximumDifferenceSimilar*MaximumDifferenceSimilar)
		{
			return false;
		}
		return true;
	}
}
