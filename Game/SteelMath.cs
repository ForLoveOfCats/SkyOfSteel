using Godot;


public class SteelMath
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
}
