using Godot;
using System.Collections.Generic;



//Naive A* implementation with Eurler distance as the heuristic
//Very WIP
public class Pathfinding
{
	public class PointData
	{
		public Vector3 Pos {get; private set;}
		public List<PointData> Friends {get; private set;}
		public PointData Parent = null;
		public float GCost {get; private set;} = 0;
		public float HCost {get; private set;} = 0;
		public float FCost {get; private set;} = 0;

		public PointData(Vector3 PositionArg)
		{
			Pos = PositionArg;
			Friends = new List<PointData>();
		}


		public void Update(float GCostArg, float HCostArg, PointData ParentArg)
		{
			Parent = ParentArg;

			GCost = GCostArg;
			HCost = HCostArg;
			FCost = GCost + HCost;
		}
	}



	public List<PointData> Points {get; private set;} = new List<PointData>();

	public Pathfinding()
	{}


	public PointData AddPoint(Vector3 Position)
	{
		var Point = new PointData(Position);
		Points.Add(Point);
		return Point;
	}


	public void ConnectPoints(PointData A, PointData B)
	{
		if(!A.Friends.Contains(B))
			A.Friends.Add(B);

		if(!B.Friends.Contains(A))
			B.Friends.Add(A);
	}


	public void RemovePoint(PointData Point)
	{
		foreach(PointData Friend in Point.Friends)
			Friend.Friends.Remove(Point);

		Points.Remove(Point);
	}


	public void Clear()
	{
		Points.Clear();
	}


	public PointData GetClosestPoint(Vector3 Position)
	{
		if(Points.Count <= 0)
			return null; //TODO: Throw an exception instead?

		PointData Closest = Points[0];
		foreach(PointData Point in Points)
		{
			if(Point.Pos.DistanceTo(Position) <= Closest.Pos.DistanceTo(Position))
				Closest = Point;
		}

		return Closest;
	}


	public List<PointData> PlotPath(PointData From, PointData To)
	{
		var OpenList = new List<PointData>();
		var ClosedList = new List<PointData>();

		From.Update(0, 0, null);
		OpenList.Add(From);

		while(true)
		{
			if(OpenList.Count <= 0)
				return new List<PointData>(); //No path

			PointData Lowest = OpenList[0];
			foreach(PointData Entry in OpenList)
			{
				if(Entry.FCost < Lowest.FCost)
					Lowest = Entry;
			}

			if(Lowest == To) //We found our path
			{
				var Path = new List<PointData>();

				PointData Current = To;
				while(true)
				{
					if(Current == From)
					{
						break;
					}

					Path.Add(Current);
					Current = Current.Parent;
				}

				return Path;
			}

			ClosedList.Add(Lowest);
			OpenList.Remove(Lowest);

			foreach(PointData Friend in Lowest.Friends)
			{
				if(ClosedList.Contains(Friend))
					continue;

				if(OpenList.Contains(Friend))
				{
					var NewGCost = Friend.Pos.DistanceTo(Lowest.Pos) + Lowest.GCost;
					if(NewGCost < Friend.GCost)
						Friend.Update(NewGCost, Friend.HCost, Lowest);
				}
				else
				{
					Friend.Update(Friend.Pos.DistanceTo(Lowest.Pos) + Lowest.GCost, Friend.Pos.DistanceTo(To.Pos), Lowest);
					OpenList.Add(Friend);
				}
			}
		}
	}
}
