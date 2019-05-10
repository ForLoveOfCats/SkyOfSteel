using System.Collections.Generic;
public class Registry<T> 
{

	//Represents the entires in a registry.
	private Dictionary<Identifier, T> entires = new Dictionary<Identifier, T>();
	//Default value for a registry if something is not registered to a certain identifier.
	private Identifier defaultID;
	
	//Indexer for more ergonomic access of entries.
	public T this[Identifier id]
	{
		get {  
			if (entires.ContainsKey(id))
			{
				return entires[id];
			}
			else {
				return entires[defaultID];
			}
		}
		set {
			//Cannot use the default identifier.
			if (id == Identifier.MISSINGNO)
			{
				Godot.GD.Print("Error: Attempted to register object under id skyofsteel:missingno.");
				return;
			}

			//Protecting against the same thing being registered multiple times.
			foreach (KeyValuePair<Identifier, T> entry in entires)
			{
				if (entry.Value as object == value as object)
				{
					Godot.GD.Print("Error: Attempted to register an object to the same registry multiple times.");
					return;
				}
			}


			entires[id] = value;
		 }
	}

	//Constructor
	public Registry(Identifier defaultid, T registryDefault)
	{
		defaultID = defaultid;
		entires[defaultid] = registryDefault;
	}


	//Gets all the entries in a given domain.
	public List<T> byDomain(string dom)
	{
		var results = new List<T>();

		foreach (KeyValuePair<Identifier, T> entry in entires)
		{
			if (entry.Key.domain == dom)
			{
				results.Add(entry.Value);
			}
		}

		return results;
	}

	public Identifier getID(T obj)
	{
		foreach (KeyValuePair<Identifier, T> entry in entires)
		{
			if (entry.Value as object == obj as object)
			{
				return entry.Key;
			}
		}

		return Identifier.MISSINGNO;
	}

	//Just a test for registires
	public static void Test() {
		Registry<Items.TYPE> testRegistry = new Registry<Items.TYPE>("skyofsteel:error", Items.TYPE.ERROR);

		testRegistry["skyofsteel:platform"] = Items.TYPE.PLATFORM;
		testRegistry["skyofsteel:slope"] = Items.TYPE.SLOPE;
		testRegistry["skyofsteel:wall"] = Items.TYPE.WALL;
		testRegistry["skyofsteel:platform"] = Items.TYPE.PLATFORM;
	}
}