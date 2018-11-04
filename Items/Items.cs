using Godot;


public class ItemInstance
{
	Items.TYPE Type = Items.TYPE.ERROR;
	int Temperature = 0;
	int Count = 0;
	int UsesRemaining = 0;
	string Description = "This is an error item and should not exist.";
}


public class Items : Node
{
	public enum TYPE {NULL, ERROR}

	private static Dictionary<string, Image> Thumbnails = new Dictionary<string, Image>();

	Items()
	{
		foreach(TYPE Type in System.Enum.GetValues(typeof(TYPE)))
		{
			object Loaded = GD.Load("res://Items/Thumbnails/" + Type.ToString() + ".png");
			if(Loaded == null)
			{
				GD.Print("No thumbnail for item '" + Type.ToString() + "'");
				throw new System.Exception("No thumbnail for item '" + Type.ToString() + "'");
			}
			Thumbnails.Add(Type.ToString(), Loaded as Image);
		}
	}
}
