using Jurassic;
using Jurassic.Library;
using Godot;

public class JsVector3Constructor : ClrFunction
{

    public JsVector3Constructor(ScriptEngine engine) 
    : base(engine.Function.InstancePrototype, "Vector3", new JsVector3Instance(engine.Object.InstancePrototype))
    {

    }

    [JSConstructorFunction]
    public JsVector3Instance Construct(double x, double y, double z) 
    {
        return new JsVector3Instance(this.InstancePrototype, x, y, z);
    }

}


public class JsVector3Instance : ObjectInstance
{

    public JsVector3Instance(ObjectInstance prototype) : base(prototype) 
    {
        GD.Print("Hello, I'm a JsVector3!");
        this["x"] = 0;
        this["y"] = 0;
        this["z"] = 0;
    }
    
    public JsVector3Instance(ObjectInstance prototype, double x, double y, double z) 
    : base(prototype)
     {
        GD.Print("Hello, I'm a JsVector3!");
        this["x"] = x;
        this["y"] = y;
        this["z"] = z;
    }
}