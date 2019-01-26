using Godot;
using System.Collections.Generic;


public class Items : Node
{
	public class Instance
	{
		public Items.TYPE Type = Items.TYPE.ERROR;
		public int Temperature = 0;
		public int Count = 1;
		public int UsesRemaining = 0;
		public string Description = "This item description is a bug and should not exists";

		public Instance(Items.TYPE TypeArg)
		{
			this.Type = TypeArg;
		}
	}


	public enum TYPE {ERROR, PLATFORM, WALL, SLOPE}

	private static Dictionary<TYPE, Texture> Thumbnails = new Dictionary<TYPE, Texture>();

	Items()
	{
		if(Engine.EditorHint) {return;}

		foreach(TYPE Type in System.Enum.GetValues(typeof(TYPE)))
		{
			object Loaded = GD.Load("res://Items/Thumbnails/" + Type.ToString() + ".png");
			if(Loaded == null)
			{
				GD.Print("No thumbnail for item '" + Type.ToString() + "'");
				throw new System.Exception("No thumbnail for item '" + Type.ToString() + "'");
			}
			Thumbnails.Add(Type, Loaded as Texture);
		}
	}


	public static Texture Thumbnail(TYPE Type)
	{
		return Thumbnails[Type];
	}
}
