public class EventObject
{
	public Events.INVOKER Invoker;
	public Events.TYPE Type;
	public object [] Args;

	public EventObject(Events.INVOKER InvokerArg, Events.TYPE TypeArg, object[] ArgsArg)
	{
		this.Invoker = InvokerArg;
		this.Type = TypeArg;
		this.Args = ArgsArg;
	}
}
