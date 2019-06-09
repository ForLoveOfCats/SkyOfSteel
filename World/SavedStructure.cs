using Godot;
using System;
using System.Linq;


public class SavedStructure
{
	public int T = (int)Items.ID.ERROR;
	public float[] P;
	public float[] R;

	public SavedStructure(Items.ID Type, Vector3 Position, Vector3 Rotation)
	{
		this.T = (int)Type;
		this.P = new float[3] {Position.x, Position.y, Position.z};
		this.R = new float[3] {Rotation.x, Rotation.y, Rotation.z};

		for(int i = 0; i <= 2; i++)
		{
			P[i] = (float)Math.Round(P[i]);
			R[i] = (float)Math.Round(R[i]);
		}
	}


	public string ToJson()
	{
		if(Enumerable.SequenceEqual(R, new float[] {0f,0f,0f}))
		{
			return $"{{\"T\":{T},\"P\":[{string.Join(",", P)}]}}";
		}
		else
		{
			return $"{{\"T\":{T},\"P\":[{string.Join(",", P)}],\"R\":[{string.Join(",", R)}]}}";
		}
	}


	public Tuple<Items.ID,Vector3,Vector3> GetInfoOrNull()
	{
		//Returns null if data is invalid

		if(P.Length != 3)
		{
			return null;
		}
		Vector3 Pos = new Vector3(P[0], P[1], P[2]);

		if(R.Length != 3)
		{
			return null;
		}
		Vector3 Rot = new Vector3(R[0], R[1], R[2]);

		return new Tuple<Items.ID,Vector3,Vector3>((Items.ID)T, Pos, Rot);
	}
}
