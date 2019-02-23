using Jurassic;
using Jurassic.Library;

public class JSVector3Constructor : ClrFunction
{

    public JSVector3Constructor(ScriptEngine engine) 
    : base(engine.Function.InstancePrototype, "Vector3", new JSVector3Instance(engine.Object.InstancePrototype))
    {

    }

    [JSConstructorFunction]
    public JSVector3Instance Construct(double x, double y, double z) {
        return new JSVector3Instance(this.InstancePrototype, x, y, z);
    }

}

public class JSVector3Instance : ObjectInstance
{

    public JSVector3Instance(ObjectInstance prototype) : base(prototype) {
        this["x"] = 0;
        this["y"] = 0;
        this["z"] = 0;
    }
    
    public JSVector3Instance(ObjectInstance prototype,double x, double y, double z) : base(prototype) {
        this["x"] = x;
        this["y"] = y;
        this["z"] = z;
    }
}