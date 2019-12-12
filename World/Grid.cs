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

	private HashSet<Vector3> QueuedUpdates = new HashSet<Vector3>();
	private HashSet<IInGrid> QueuedRemovals = new HashSet<IInGrid>();
	private Dictionary<IInGrid, List<Vector3>> QueuedRemovalAreas = new Dictionary<IInGrid, List<Vector3>>();


	public static Vector3 CalculateArea(Vector3 Position)
	{
		return new Vector3(RoundToInt(Position.x/PlatformSize)*PlatformSize,
						   RoundToInt(Position.y/PlatformSize)*PlatformSize,
						   RoundToInt(Position.z/PlatformSize)*PlatformSize);
	}


	public static List<Vector3> CalculateAreas(Vector3 Position)
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
		if(QueuedRemovals.Contains(Item))
		{
			QueuedRemovals.Remove(Item);
			QueuedRemovalAreas.Remove(Item);
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


	//Items cannot be removed from the grid while updating
	//as we cannot modify the List while foreaching it
	public void QueueRemoveItem(IInGrid Item)
	{
		QueuedRemovals.Add(Item);
		QueuedRemovalAreas[Item] = CalculateAreas(Item.Translation);
	}


	//Must be called periodicly
	private void DoRemoves()
	{
		foreach(IInGrid Item in QueuedRemovals)
		{
			foreach(Vector3 Area in QueuedRemovalAreas[Item])
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
		QueuedRemovals.Clear();
		QueuedRemovalAreas.Clear();
	}


	public void QueueUpdateArea(Vector3 Position)
	{
		QueuedUpdates.Add(CalculateArea(Position));
	}


	public void QueueUpdateNearby(Vector3 Position)
	{
		foreach(Vector3 CorePos in CalculateAreas(Position))
		{
			for(float MulX = -1; MulX <= 1; MulX++)
			{
				for(float MulY = -1; MulY <= 1; MulY++)
				{
					for(float MulZ = -1; MulZ <= 1; MulZ++)
					{
						QueuedUpdates.Add(CorePos + new Vector3(MulX*PlatformSize, MulY*PlatformSize, MulZ*PlatformSize));
					}
				}
			}
		}
	}


	//Must be called periodicly
	private void DoUpdates()
	{
		foreach(Vector3 Area in QueuedUpdates)
		{
			foreach(IInGrid Item in GetItems(Area))
			{
				if(Item is Tile Branch && Branch.Point != null)
				{
					World.Pathfinder.RemovePoint(Branch.Point);
					World.TryAddTileToPathfinder(Branch);
				}

				Item.GridUpdate();
			}
		}
		QueuedUpdates.Clear();
	}


	//Must be called periodicly from World
	public void DoWork()
	{
		DoRemoves();
		DoUpdates();
	}


	public void Clear()
	{
		Dict.Clear();
	}
}
