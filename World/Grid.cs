using Godot;
using static Godot.Mathf;
using System.Collections.Generic;


public interface IInGrid
{
	Vector3 Translation { get; set; }

	void GridUpdate();
}



public class GridClass
{
	private const int PlatformSize = World.PlatformSize;
	private Dictionary<Vector3, List<IInGrid>> Dict = new Dictionary<Vector3, List<IInGrid>>();

	private List<IInGrid> Removals = new List<IInGrid>();


	private Vector3 CalculateArea(Vector3 Position)
	{
		return new Vector3(FloorToInt(Position.x/PlatformSize)*PlatformSize,
						   FloorToInt(Position.y/PlatformSize)*PlatformSize,
						   FloorToInt(Position.z/PlatformSize)*PlatformSize);
	}


	private List<Vector3> CalculateAreas(Vector3 Position)
	{
		List<Vector3> Out = new List<Vector3>();
		Out.Add(CalculateArea(Position));

		if(Position.x % PlatformSize != 0)
		{
			Out.Add(CalculateArea(Position + new Vector3(PlatformSize/2,0,0)));
		}
		else if(Position.z % PlatformSize != 0)
		{
			Out.Add(CalculateArea(Position + new Vector3(0,0,PlatformSize/2)));
		}

		return Out;
	}


	public void AddItem(IInGrid Item)
	{
		if(Removals.Contains(Item))
		{
			Removals.Remove(Item);
		}
		else
		{
			List<IInGrid> Items;
			foreach(Vector3 Area in CalculateAreas(Item.Translation))
			{
				Dict.TryGetValue(Area, out Items);
				if(Items == null)
				{
					Items = new List<IInGrid>() {Item};
				}
				else if(!Items.Contains(Item))
				{
					Items.Add(Item);
				}
				Dict[Area] = Items;
			}
		}
	}


	//Items cannot be removed from the grid while updating
	//as we cannot modify the List while foreaching it
	public void QueueRemoveItem(IInGrid Item)
	{
		Removals.Add(Item);
	}


	//Must be called periodicly
	public void DoRemoves()
	{
		foreach(IInGrid Item in Removals)
		{
			foreach(Vector3 Area in CalculateAreas(Item.Translation))
			{
				List<IInGrid> Items;
				Dict.TryGetValue(Area, out Items);

				if(Items != null)
				{
					Items.Remove(Item);

					if(Items.Count <= 0)
					{
						Dict.Remove(Area);
					}
					else
					{
						Dict[Area] = Items;
					}
				}
			}
		}
		Removals.Clear();
	}


	public List<IInGrid> GetItems(Vector3 Position)
	{
		List<IInGrid> Items;
		Dict.TryGetValue(CalculateArea(Position), out Items);

		if(Items == null)
		{
			return new List<IInGrid>() {};
		}
		return Items;
	}


	public void UpdateArea(Vector3 Position)
	{
		foreach(IInGrid Item in GetItems(Position))
		{
			Item.GridUpdate();
		}
	}


	public void UpdateNearby(Vector3 Position)
	{
		HashSet<Vector3> Areas = new HashSet<Vector3>();

		foreach(Vector3 CorePos in CalculateAreas(Position))
		{
			for(float MulX = -1; MulX <= 1; MulX++)
			{
				for(float MulY = -1; MulY <= 1; MulY++)
				{
					for(float MulZ = -1; MulZ <= 1; MulZ++)
					{
						Areas.Add(CorePos + new Vector3(MulX*PlatformSize, MulY*PlatformSize, MulZ*PlatformSize));
					}
				}
			}
		}

		foreach(Vector3 Area in Areas)
		{
			UpdateArea(Area);
		}
	}


	public void Clear()
	{
		Dict.Clear();
	}
}
