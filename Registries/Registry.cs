using System.Collections.Generic;
class Registry<T> 
{

	//Represents the entires in a registry.
	private Dictionary<Identifier, T> entires = new Dictionary<Identifier, T>();
	//Default value for a registry if something is not registered to a certain identifier.
	private T defaultVal;
	
	//Indexer for more ergonomic access of entries.
	public T this[Identifier id]
	{
		get {  
			if (entires.ContainsKey(id))
			{
				return entires[id];
			}
			else {
				return defaultVal;
			}
		}
		set {
			//Cannot use the default identifier.
			if (id == Identifier.MISSINGNO)
			{
				Console.Log("Cannot register objects using identifier skyofsteel:missingo.");
				return;
			}

			//Protecting against the same thing being registered multiple times.
			foreach (KeyValuePair<Identifier, T> entry in entires)
			{
				if (entry.Value as object == value as object)
				{
					Console.Log("Cannot register objects to the same registry multiple times.");
					return;
				}
			}


			entires[id] = value;
		 }
	}

	//Constructor
	public Registry(T registryDefault)
	{
		defaultVal = registryDefault;
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
}