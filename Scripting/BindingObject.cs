class BindingObject
{
	public string Name = null; //Null to fail early
	public string Function = null; //Null to fail early
	public Bindings.TYPE Type = Bindings.TYPE.UNSET;
	public enum DIRECTION {UP, DOWN, RIGHT, LEFT};
	public DIRECTION AxisDirection; //Only used if Type is AXIS

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
