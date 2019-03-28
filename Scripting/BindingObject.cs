using System;


class BindingObject
{
	public string Name = null; //Null to fail early
	public Action FuncWithoutArg = null;
	public Action<float> FuncWithArg = null;
	public Bindings.TYPE Type = Bindings.TYPE.UNSET;
	public Bindings.DIRECTION AxisDirection; //Only used if Type is AXIS

	public bool JoyWasInDeadzone = true;

	public BindingObject(string NameArg)
	{
		Name = NameArg;
	}


	public bool Equals(BindingObject Other)
	{
		return Name == Other.Name;
	}
}
