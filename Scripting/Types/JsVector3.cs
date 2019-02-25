using Jurassic;
using Jurassic.Library;
using Godot;


public class JsVector3Constructor : ClrFunction
{
	public JsVector3Constructor(ScriptEngine engine)
		: base(engine.Function.InstancePrototype, "Vector3", new JsVector3(engine.Object.InstancePrototype))
	{}


	[JSConstructorFunction]
	public JsVector3 Construct()
	{
		return new JsVector3(this.InstancePrototype);
	}


	[JSConstructorFunction]
	public JsVector3 Construct(double x, double y, double z)
	{
		return new JsVector3(this.InstancePrototype, x, y, z);
	}
}


public class JsVector3 : ObjectInstance
{
	public JsVector3(ObjectInstance prototype) : base(prototype)
	{
		this["x"] = 0;
		this["y"] = 0;
		this["z"] = 0;
	}


	public JsVector3(ObjectInstance prototype, Vector3 InVec) : base(prototype)
	{
		this["x"] = (double)InVec.x;
		this["y"] = (double)InVec.y;
		this["z"] = (double)InVec.z;
	}


	public JsVector3(ObjectInstance prototype, double x, double y, double z) : base(prototype)
	{
		this["x"] = x;
		this["y"] = y;
		this["z"] = z;
	}


	public Vector3 Sharpen()
	{
		return new Vector3((float)this["x"], (float)this["y"], (float)this["z"]);
	}
}
