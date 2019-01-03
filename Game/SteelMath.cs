using Godot;


public class SteelMath
{
	public static float SnapToGrid(float ToSnap, int GridSize, int DivisionCount)
	{
		return Mathf.Round(ToSnap/(GridSize/DivisionCount))*(GridSize/DivisionCount);
	}


	public static float LoopRotation(float Rot) 
	{
		while(Rot > 360)
		{
			Rot -= 360;
		}

		while(Rot < 0)
		{
			Rot += 360;
		}

		if(Rot == 360)
		{
			Rot = 0;
		}

		return Rot;
	}
}
