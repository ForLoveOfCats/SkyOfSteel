using Godot;
using static Godot.Mathf;
using System.Collections.Generic;
using Optional;



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


		public static Option<PointData> None()
		{
			return Option.None<PointData>();
		}


		public Option<PointData> Some()
		{
			return Option.Some(this);
		}
	}



	public class PointDataHeap
	{
		//List because it needs to have dynamic length
		private List<PointData> Representation = new List<PointData>();
		private HashSet<PointData> LookupSet = new HashSet<PointData>();

		public int Count {
			get {
				return Representation.Count;
			}
		}


		public PointDataHeap()
		{}


		public bool Contains(PointData Point)
		{
			return LookupSet.Contains(Point);
		}


		public void Add(PointData Point)
		{
			LookupSet.Add(Point);

			Representation.Add(Point);

			int Index = Representation.LastIndex();
			int ParentIndex = ParentOf(Index);
			while(ParentIndex >= 0 && Representation[ParentIndex].FCost > Point.FCost)
			{
				SwapAt(Index, ParentIndex);
				Index = ParentIndex;
				ParentIndex = ParentOf(Index);
			}
		}


		public PointData GetMin()
		{
			return Representation[0];
		}


		public void RemoveMin()
		{
			LookupSet.Remove(Representation[0]);

			Representation[0] = Representation.Last();
			Representation.RemoveAt(Representation.LastIndex());

			int Index = 0;
			int FirstChildIndex  = FirstChildOf(Index);
			int SecondChildIndex = SecondChildOf(Index);
			int MinIndex = Index;
			while(true)
			{
				if(SecondChildIndex >= Representation.Count)
				{
					if(FirstChildIndex >= Representation.Count)
						return;
					MinIndex = FirstChildIndex;
				}
				else
				{
					if(Representation[FirstChildIndex].FCost < Representation[SecondChildIndex].FCost)
						MinIndex = FirstChildIndex;
					else
						MinIndex = SecondChildIndex;
				}

				if(Representation[Index].FCost > Representation[MinIndex].FCost)
				{
					SwapAt(Index, MinIndex);
					Index = MinIndex;

					FirstChildIndex  = FirstChildOf(Index);
					SecondChildIndex = SecondChildOf(Index);
				}
				else
					return;
			}
		}


		private void SwapAt(int A, int B)
		{
			var ActualA = Representation[A];
			var ActualB = Representation[B];

			Representation[A] = ActualB;
			Representation[B] = ActualA;
		}


		private int ParentOf(int Index)
		{
			return FloorToInt((Index-1) / 2);
		}


		private int FirstChildOf(int Index)
		{
			return FloorToInt((Index*2) + 1);
		}


		private int SecondChildOf(int Index)
		{
			return FloorToInt((Index*2) + 2);
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
		var Friends = new List<PointData>();
		foreach(PointData Friend in Point.Friends)
			Friends.Add(Friend);

		foreach(PointData Friend in Friends)
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
		var OpenList = new PointDataHeap();
		var ClosedList = new HashSet<PointData>();

		From.Update(0, 0, null);
		OpenList.Add(From);

		while(true)
		{
			if(OpenList.Count <= 0)
				return new List<PointData>(); //No path

			PointData Lowest = OpenList.GetMin();

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
			OpenList.RemoveMin();

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
