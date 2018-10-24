class BindingObject
{
	public string Name = "";
	public Bindings.BIND_TYPE Type = Bindings.BIND_TYPE.SCANCODE;
	public enum DIRECTION {UP, DOWN, RIGHT, LEFT};
	public DIRECTION AxisDirection;

	public BindingObject(string NameArg, Bindings.BIND_TYPE TypeArg)
	{
		this.Name = NameArg;
		this.Type = TypeArg;
	}


	public bool Equals(BindingObject Other)
	{
		return this.Name == Other.Name;
	}
}
