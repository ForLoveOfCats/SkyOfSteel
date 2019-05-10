using System.Collections.Generic;
class Identifier
{
	public string domain;
	public string name;

	public static readonly Identifier MISSINGNO = new Identifier("missingno");

	public Identifier(string id)
	{
		//Splits the string into a domain and name components.
		string[] parts = id.Split(':');

		if (parts.Length == 1)
		{
			//If there is no domain, the domain is set to the vanilla domain (skyofsteel).
			domain = "skyofsteel";
			name = parts[0];
		}
		else
		{
			domain = parts[0];
			name = parts[1];
		}
	}

	public override string ToString()
	{
		return domain + ":" + name;
	}

	public static bool operator== (Identifier left, Identifier right)
	{
		return (left.domain == right.domain) && (left.name == right.name);
	}

	public static bool operator!= (Identifier left, Identifier right)
	{
		return (left.domain != right.domain) || (left.name != right.name);
	}

	public override bool Equals(object obj)
	{
		if (obj is Identifier) {
			Identifier identifier = obj as Identifier;
			return (domain == identifier.domain) && (name == identifier.name);
		}

		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}