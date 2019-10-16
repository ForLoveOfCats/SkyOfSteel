using Godot;
using static Godot.Mathf;


public static class SteelMath
{
	public static float SnapToGrid(float ToSnap, int GridSize, int DivisionCount)
	{
		return Mathf.Round(ToSnap/(GridSize/DivisionCount))*(GridSize/DivisionCount);
	}


	public static float LoopRotation(float Rot)
	{
		Rot = Rot % 360;

		if(Rot < 0)
			Rot += 360;

		if(Rot == 360)
			Rot = 0;

		return Rot;
	}


	public static Vector3 ClampVec3(Vector3 Vec, float Min, float Max)
	{
		return Vec.Normalized() * Mathf.Clamp(Vec.Length(), Min, Max);
	}


	public static Vector3 RoundVec3(Vector3 Vec)
	{
		return new Vector3(Round(Vec.x), Round(Vec.y), Round(Vec.z));
	}


	public static Vector3 Flattened(this Vector3 Self)
	{
		return new Vector3(Self.x, 0, Self.z);
	}


	public static Vector2 ClampVec2(Vector2 Vec, float Min, float Max)
	{
		return Vec.Normalized() * Mathf.Clamp(Vec.Length(), Min, Max);
	}
}
