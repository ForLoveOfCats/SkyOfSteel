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


	public void AddItem(IInGrid Item)
	{
		List<IInGrid> Items;
		foreach(Vector3 Area in GetAreas(Item.Translation))
		{
			Dict.TryGetValue(Area, out Items);
			if(Items == null)
			{
				Items = new List<IInGrid>() {Item};
			}
			else
			{
				Items.Add(Item);
			}
			Dict[Area] = Items;
		}
	}


	public void RemoveItem(IInGrid Item)
	{
		foreach(Vector3 Area in GetAreas(Item.Translation))
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


	public List<IInGrid> GetItems(Vector3 Position)
	{
		List<IInGrid> Items;
		Dict.TryGetValue(Position, out Items);

		if(Items == null)
		{
			return new List<IInGrid>() {};
		}
		return Items;
	}


	public void Clear()
	{
		Dict.Clear();
	}
}
