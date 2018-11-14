using Godot;


public class Building : Node
{
	private static Dictionary<Items.TYPE, PackedScene> Scenes = new Dictionary<Items.TYPE, PackedScene>();

	Building()
	{
		foreach(Items.TYPE Type in System.Enum.GetValues(typeof(Items.TYPE)))
		{
			File ToLoad = new File();
			if(ToLoad.FileExists("res://Building/Scenes/" + Type.ToString() + ".tscn"))
			{
				Scenes.Add(Type, GD.Load("res://Building/Scenes/" + Type.ToString() + ".tscn") as PackedScene);
			}
			else
			{
				Scenes.Add(Type, GD.Load("res://Building/Scenes/ERROR.tscn") as PackedScene);
			}
		}
	}


	public static void PositionCalculate()
	{
	}


	public static void RotationCalculate()
	{
	}


	public static void Place(Items.TYPE Type, Vector3 Position, Vector3 Rotation, int OwnerId)
	{
		System.Console.WriteLine(Type.ToString());
		Spatial Structure = Scenes[Type].Instance() as Spatial;
		Structure.Translation = Position;
		Structure.RotationDegrees = Rotation;
		Structure.SetName(System.Guid.NewGuid().ToString()); //name can be used to reference a structure over network

		Game.StructureRoot.AddChild(Structure);
	}
}
