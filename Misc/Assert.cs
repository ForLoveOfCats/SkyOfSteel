using System;



public static class Assert {
	public class AssertException : Exception {
		public AssertException() { }

		public AssertException(string Message) : base(Message) { }
	}


	//Because C# is a silly language where Debug.Assert doesn't quit or throw an exception on failure
	public static void ActualAssert(bool Condition) {
		if(!Condition)
			throw new AssertException();
	}


	public static void ArgArray(object[] Array, params Type[] Types) {
		if(Array.Length != Types.Length)
			throw new AssertException($"Incorrect argument count, expected {Types.Length} but got {Array.Length}");

		int Index = 0;
		foreach(object Item in Array) {
			var Expected = Types[Index];
			var Actual = Item.GetType();
			if(Actual != Expected)
				throw new AssertException($"The {Index}-th argument had incorrect type, expected {Expected} but go {Actual}");

			Index += 1;
		}
	}
}
