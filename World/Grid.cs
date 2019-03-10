using Godot;
using static Godot.Mathf;
using System.Collections.Generic;


public class GridClass
{
	private const int PlatformSize = World.PlatformSize;
	private Dictionary<Vector3, List<Structure>> Dict = new Dictionary<Vector3, List<Structure>>();


	private Vector3 GetArea(Vector3 Position)
	{
		return new Vector3(FloorToInt(Position.x/PlatformSize)*PlatformSize,
						   FloorToInt(Position.y/PlatformSize)*PlatformSize,
						   FloorToInt(Position.z/PlatformSize)*PlatformSize);
	}


	private List<Vector3> GetAreas(Vector3 Position)
	{
		List<Vector3> Out = new List<Vector3>();
		Out.Add(GetArea(Position));

		if(Position.x % PlatformSize != 0)
		{
			Out.Add(GetArea(Position + new Vector3(PlatformSize/2,0,0)));
		}
		else if(Position.z % PlatformSize != 0)
		{
			Out.Add(GetArea(Position + new Vector3(0,0,PlatformSize/2)));
		}

		return Out;
	}


	private void AddBranch(ref List<Structure> Branches, Structure Branch)
	{
		if(Branches == null)
		{
			Branches = new List<Structure>() {Branch};
		}
		else
		{
			Branches.Add(Branch);
		}
	}


	public void Add(Structure Branch)
	{
		List<Structure> Branches;

		foreach(Vector3 Area in GetAreas(Branch.Translation))
		{
			Dict.TryGetValue(Area, out Branches);
			AddBranch(ref Branches, Branch);
			Dict[Area] = Branches;
		}
	}


	public void Remove(Structure Branch)
	{
		foreach(Vector3 Area in GetAreas(Branch.Translation))
		{
			List<Structure> Branches;
			Dict.TryGetValue(Area, out Branches);

			if(Branches != null)
			{
				Branches.Remove(Branch);

				if(Branches.Count <= 0)
				{
					Dict.Remove(Area);
				}
				else
				{
					Dict[Area] = Branches;
				}
			}
		}
	}


	public void Clear()
	{
		Dict.Clear();
	}


	public List<Structure> Get(Vector3 Position)
	{
		List<Structure> Branches;
		Dict.TryGetValue(Position, out Branches);

		if(Branches == null)
		{
			return new List<Structure>() {};
		}
		return Branches;
	}
}
