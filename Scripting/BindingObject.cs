class BindingObject
{
	public string Name = null; //Null to fail early
	public string Function = null; //Null to fail early
	public Bindings.TYPE Type = Bindings.TYPE.UNSET;
	public Bindings.DIRECTION AxisDirection; //Only used if Type is AXIS

	public bool JoyWasInDeadzone = true;

	public BindingObject(string NameArg, string FunctionArg)
	{
		Name = NameArg;
		Function = FunctionArg;
	}


	public bool Equals(BindingObject Other)
	{
		return Name == Other.Name;
	}
}
