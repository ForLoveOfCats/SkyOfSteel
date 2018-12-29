using Godot;


public class SavedStructure
{
	public int T;
	public float[] P;
	public float[] R;

	public SavedStructure(Items.TYPE Type, Vector3 Position, Vector3 Rotation)
	{
		this.T = (int)Type;
		this.P = new float[3] {Position.x, Position.y, Position.z};
		this.R = new float[3] {Rotation.x, Rotation.y, Rotation.z};
	}


	public string ToJson()
	{
		return $"{{\"T\":{T},\"P\":[{string.Join(",", P)}],\"R\":[{string.Join(",", R)}]}}";
	}


	public Structure ToStructureOrNull()
	{
		//If cannot make a valid instance then return null
		Structure Branch = Building.Scenes[(Items.TYPE)T].Instance() as Structure;

		Branch.Type = (Items.TYPE)T;

		if(P.Length != 3)
		{
			return null;
		}
		Branch.Translation = new Vector3(P[0], P[1], P[2]);

		if(R.Length != 3)
		{
			return null;
		}
		Branch.RotationDegrees = new Vector3(R[0], R[1], R[2]);

		return Branch;
	}
}
