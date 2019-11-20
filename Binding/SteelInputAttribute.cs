using System;
using System.Reflection;



public class SteelInputWithoutArg : Attribute
{
	public Action Function;


	public SteelInputWithoutArg(Type T, string FunctionName)
	{
		MethodInfo Method = T.GetMethod(FunctionName);
		if(!Method.IsStatic)
			throw new Exception($"Method {FunctionName} is not static");
		if(Method.GetParameters().Length > 0)
			throw new Exception($"Method {FunctionName} has arguments");

		Function = (Action)Delegate.CreateDelegate(typeof(Action), Method);
	}
}



public class SteelInputWithArg : Attribute
{
	public Action<float> Function;


	public SteelInputWithArg(Type T, string FunctionName)
	{
		MethodInfo Method = T.GetMethod(FunctionName);
		if(!Method.IsStatic)
			throw new Exception($"Method {FunctionName} is not static");

		Function = (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), Method);
	}
}
