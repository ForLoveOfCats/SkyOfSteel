using Godot;
using static Godot.Mathf;
using static SteelMath;
using System;
using System.Collections.Generic;


public class Items : Node
{
	public delegate Vector3? BuildInfoDelegate(Structure Base, float PlayerOrientation, int BuildRotation, Vector3 HitPointRelative);


	public class Instance
	{
		public Items.ID Type = Items.ID.ERROR;
		public int Temperature = 0;
		public int Count = 1;
		public int UsesRemaining = 0;

		public Instance(Items.ID TypeArg)
		{
			this.Type = TypeArg;
		}
	}


	private struct CustomItemEnum //Reference implimentation for 0.1.3
	{
		public static int NextSpot = Enum.GetNames(typeof(ID)).Length;
		public int Spot;

		public CustomItemEnum(int SpotArg)
		{
			Spot = SpotArg;
		}


		public static implicit operator ID(CustomItemEnum ItemEnum)
		{
			return (ID)(ItemEnum.Spot);
		}
	}

	private static CustomItemEnum NewCustomItemEnum() //Reference implimentation for 0.1.3
	{
		CustomItemEnum NewItemEnum = new CustomItemEnum(CustomItemEnum.NextSpot);
		CustomItemEnum.NextSpot++;
		return NewItemEnum;
	}


	public enum ID {ERROR, PLATFORM, WALL, SLOPE}

	public static Dictionary<ID, Mesh> Meshes = new Dictionary<ID, Mesh>();
	public static Dictionary<ID, Texture> Thumbnails = new Dictionary<ID, Texture>();
	public static Dictionary<ID, Texture> Textures { get; private set; } = new Dictionary<ID, Texture>();

	public static Dictionary<ID, BuildInfoDelegate> BuildPositions = new Dictionary<ID, BuildInfoDelegate>();
	public static Dictionary<ID, BuildInfoDelegate> BuildRotations = new Dictionary<ID, BuildInfoDelegate>();

	public static Shader StructureShader { get; private set; }

	Items()
	{
		if(Engine.EditorHint) {return;}

		StructureShader = GD.Load<Shader>("res://World/Materials/StructureShader.shader");

		foreach(Items.ID Type in System.Enum.GetValues(typeof(ID)))
		{
			Meshes.Add(Type, GD.Load<Mesh>($"res://Items/Meshes/{Type}.obj"));
			//Assume that every item has a mesh, will throw exception on game startup if not
		}

		foreach(ID Type in System.Enum.GetValues(typeof(ID)))
		{
			Thumbnails.Add(Type, GD.Load<Texture>($"res://Items/Thumbnails/{Type}.png"));
			//Assume that every item has a thumbnail, will throw exception on game startup if not
		}

		foreach(ID Type in System.Enum.GetValues(typeof(ID)))
		{
			Textures.Add(Type, GD.Load<Texture>($"res://Items/Textures/{Type}.png"));
			//Assume that every item has a texture, will throw exception on game startup if not
		}
	}


	public static Vector3? TryCalculateBuildPosition(ID Branch, Structure Base, float PlayerOrientation, int BuildRotation, Vector3 Hit)
	{
		BuildInfoDelegate Function;
		BuildPositions.TryGetValue(Branch, out Function);

		if(Function != null)
		{
			Vector3? PossiblePosition = Function(Base, PlayerOrientation, BuildRotation, Hit - Base.Translation);
			if(PossiblePosition is Vector3 Position) //For now round all positions until it causes issues
				return new Vector3(Round(Position.x), Round(Position.y), Round(Position.z));
		}

		return null;
	}


	public static Vector3 CalculateBuildRotation(ID Branch, Structure Base, float PlayerOrientation, int BuildRotation, Vector3 Hit) //Always return a valid rotation
	{
		BuildInfoDelegate Function;
		BuildRotations.TryGetValue(Branch, out Function);

		if(Function != null)
		{
			Vector3? PossibleRotation = Function(Base, PlayerOrientation, BuildRotation, Hit - Base.Translation);
			if(PossibleRotation is Vector3 Rotation) //For now round all rotations until it causes issues
				return new Vector3(Round(Rotation.x), Round(Rotation.y), Round(Rotation.z));
		}

		return new Vector3();
	}


	public static void SetupItems()
	{
		BuildPositions = new Dictionary<ID, BuildInfoDelegate>() {
			{
				ID.PLATFORM,
				new BuildInfoDelegate((Structure Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) => {
						switch(Base.Type)
						{
							case(ID.PLATFORM):
							{
								PlayerOrientation = Mathf.Deg2Rad(SnapToGrid(PlayerOrientation, 360, 4));
								return Base.Translation + (new Vector3(0,0,12)).Rotated(new Vector3(0,1,0), PlayerOrientation);
							}

							case(ID.WALL):
							{
								float Orientation = LoopRotation(SnapToGrid(PlayerOrientation, 360, 4) + 180);

								if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
								   && LoopRotation(Orientation+180) != LoopRotation(Round(Base.RotationDegrees.y)))
								{
									return null;
								}

								if(BuildRotation == 1 || BuildRotation == 3)
									Orientation = LoopRotation(Orientation + 180);

								int yOffset = 6;
								if(HitRelative.y + Base.Translation.y < Base.Translation.y)
									yOffset = -6;

								return new Vector3(0, yOffset, 6).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(Orientation)) + Base.Translation;
							}

							case(ID.SLOPE):
							{
								float Orientation = LoopRotation(SnapToGrid(PlayerOrientation, 360, 4));

								if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
								   && LoopRotation(Orientation+180) != LoopRotation(Round(Base.RotationDegrees.y)))
								{
									return null;
								}

								int zOffset = 12;
								if(BuildRotation == 1 || BuildRotation == 3)
									zOffset = 0;

								if(Orientation == LoopRotation(Round(Base.RotationDegrees.y)))
								{
									return new Vector3(0, 6, zOffset).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(Orientation)) + Base.Translation;
								}
								else
								{
									return new Vector3(0, -6, zOffset).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(Orientation)) + Base.Translation;
								}
							}
						}

						return null;
					})
			},

			{
				ID.WALL,
				new BuildInfoDelegate((Structure Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) => {
						switch(Base.Type)
						{
							case(ID.PLATFORM):
							{
								PlayerOrientation = Mathf.Deg2Rad(SnapToGrid(PlayerOrientation, 360, 4));

								int yOffset = 6;
								if(BuildRotation == 1 || BuildRotation == 3)
									yOffset = -6;

								return new Vector3(0, yOffset, 6).Rotated(new Vector3(0,1,0), PlayerOrientation) + Base.Translation;
							}

							case(ID.WALL):
							{
								float Orientation = LoopRotation(SnapToGrid(PlayerOrientation, 360, 4));

								if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
								   && LoopRotation(Orientation+180) != LoopRotation(Round(Base.RotationDegrees.y))) //Not facing straight on
								{
									return new Vector3(0, 0, 12).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
								}
								else
								{
									if(HitRelative.y + Base.Translation.y >= Base.Translation.y)
										return new Vector3(0, 12, 0) + Base.Translation;
									else
										return new Vector3(0, -12, 0) + Base.Translation;
								}
							}

							case(ID.SLOPE):
							{
								float Orientation = LoopRotation(SnapToGrid(PlayerOrientation, 360, 4));

								if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
								   && LoopRotation(Orientation+180) != LoopRotation(Round(Base.RotationDegrees.y)))
								{
									return null;
								}

								int yOffset = 0;
								if(BuildRotation == 1 || BuildRotation == 3)
									yOffset = -12;

								if(Orientation == LoopRotation(Round(Base.RotationDegrees.y)))
								{
									return new Vector3(0, 12 + yOffset, 6).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(Orientation)) + Base.Translation;
								}
								else
								{
									return new Vector3(0, yOffset, 6).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(Orientation)) + Base.Translation;
								}
							}
						}

						return null;
					})
			},

			{
				ID.SLOPE,
				new BuildInfoDelegate((Structure Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) => {
						switch(Base.Type)
						{
							case(ID.PLATFORM):
							{
								float Orientation = SnapToGrid(PlayerOrientation, 360, 4);

								int yOffset = 6;
								if(BuildRotation == 1 || BuildRotation == 3)
									yOffset = -6;

								return new Vector3(0, yOffset, 12).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
							}

							case(ID.WALL):
							{
								float Orientation = LoopRotation(SnapToGrid(PlayerOrientation, 360, 4));
								if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
								   && LoopRotation(Orientation+180) != LoopRotation(Round(Base.RotationDegrees.y)))
								{
									return null;
								}

								if(HitRelative.y + Base.Translation.y >= Base.Translation.y)
								{
									if(BuildRotation == 0)
										return new Vector3(0, 12, 6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
									if(BuildRotation == 1)
										return new Vector3(0, 0, 6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
									if(BuildRotation == 2)
										return new Vector3(0, 12, -6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
									if(BuildRotation == 3)
										return new Vector3(0, 0, -6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
								}
								else
								{
									if(BuildRotation == 0)
										return new Vector3(0, 0, 6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
									if(BuildRotation == 1)
										return new Vector3(0, -12, 6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
									if(BuildRotation == 2)
										return new Vector3(0, 0, -6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
									if(BuildRotation == 3)
										return new Vector3(0, -12, -6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
								}

								return null;
							}
						}

						return null;
					})
			}
		};

		BuildRotations = new Dictionary<ID, BuildInfoDelegate>() {
			{
				ID.PLATFORM,
				new BuildInfoDelegate((Structure Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) => {
						return new Vector3(); //PLATFORM will always have a rotation of 0,0,0
					})
			},

			{
				ID.WALL,
				new BuildInfoDelegate((Structure Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) => {
						if(Base.Type == ID.WALL || Base.Type == ID.SLOPE)
							return Base.RotationDegrees;

						return new Vector3(0, SnapToGrid(PlayerOrientation, 360, 4), 0);
					})
			},

			{
				ID.SLOPE,
				new BuildInfoDelegate((Structure Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) => {
						if(Base.Type == ID.PLATFORM)
							if(BuildRotation == 1 || BuildRotation == 3)
								return new Vector3(0, LoopRotation(SnapToGrid(PlayerOrientation, 360, 4) + 180), 0);

						if(Base.Type == ID.WALL)
						{
							float Orientation = SnapToGrid(PlayerOrientation, 360, 4);
							if(BuildRotation == 0)
								return new Vector3(0, SnapToGrid(Orientation, 360, 4), 0);
							if(BuildRotation == 1)
								return new Vector3(0, SnapToGrid(LoopRotation(Orientation + 180), 360, 4), 0);
							if(BuildRotation == 2)
								return new Vector3(0, SnapToGrid(LoopRotation(Orientation + 180), 360, 4), 0);
							if(BuildRotation == 3)
								return new Vector3(0, SnapToGrid(Orientation, 360, 4), 0);
						}

						return new Vector3(0, SnapToGrid(PlayerOrientation, 360, 4), 0);
					})
			},
		};
	}
}
