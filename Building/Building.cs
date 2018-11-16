using Godot;


public class Building : Node
{
	private static BuildPositions BuildPositionsInstance;
	private static BuildRotations BuildRotationsInstance;

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

		BuildPositionsInstance = new BuildPositions();
		BuildRotationsInstance = new BuildRotations();
	}


	public static Vector3 PositionCalculate(Structure Base, Items.TYPE Branch)
	{
		return BuildPositionsInstance.Calculate(Base, Branch);
	}


	public static Vector3 RotationCalculate(Structure Base, Items.TYPE Branch)
	{
		return BuildRotationsInstance.Calculate(Base, Branch);
	}


	public static void Place(Items.TYPE Type, Vector3 Position, Vector3 Rotation, int OwnerId)
	{
		Structure Branch = Scenes[Type].Instance() as Structure;
		Branch.Type = Type;
		Branch.OwnerId = OwnerId;
		Branch.Translation = Position;
		Branch.RotationDegrees = Rotation;
		Branch.SetName(System.Guid.NewGuid().ToString()); //name can be used to reference a structure over network

		Game.StructureRoot.AddChild(Branch);
	}
}
