using Godot;


public class SteelMath
{
	public static float SnapToGrid(float ToSnap, int GridSize, int DivisionCount)
	{
		return Mathf.Round(ToSnap/(GridSize/DivisionCount))*(GridSize/DivisionCount);
	}
}
