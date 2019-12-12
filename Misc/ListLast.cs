using System.Collections.Generic;



public static class ListLastClass
{
	public static T Last<T>(this List<T> Self)
	{
		return Self[Self.Count - 1];
	}


	public static int LastIndex<T>(this List<T> Self)
	{
		return Self.Count - 1;
	}
}
