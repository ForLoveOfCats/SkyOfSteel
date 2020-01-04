using System;



public static class Assert
{
	public class AssertException : Exception
	{}


	//Because C# is a silly language where Debug.Assert doesn't quit or throw an exception on failure
	public static void ActualAssert(bool Condition)
	{
		if(!Condition)
			throw new AssertException();
	}
}
