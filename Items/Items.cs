using Godot;
using static Godot.Mathf;
using static SteelMath;
using System;
using System.Collections.Generic;
using static System.Diagnostics.Debug;


public class Items : Node
{
	public delegate Vector3? BuildInfoDelegate(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative);
	public delegate void UseItemDelegate(Instance Item, Player UsingPlayer);


	public class Instance
	{
		public Items.ID Id = Items.ID.ERROR;
		public int Temperature = 0;
		public int Count = 1;
		public int UsesRemaining = 0;

		public Instance(Items.ID IdArg)
		{
			Id = IdArg;
		}
	}


	public struct IdInfo
	{
		public BuildInfoDelegate PositionDelegate;
		public BuildInfoDelegate RotationDelegate;

		public UseItemDelegate UseDelegate;
		public bool CanAds;

		public ID[] DisallowedCollisions;
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


	public enum ID {ERROR, PLATFORM, WALL, SLOPE, TRIANGLE_WALL, ROCKET_JUMPER, THUNDERBOLT}

	public static Dictionary<ID, Mesh> Meshes = new Dictionary<ID, Mesh>();
	public static Dictionary<ID, Texture> Thumbnails = new Dictionary<ID, Texture>();
	public static Dictionary<ID, Texture> Textures { get; private set; } = new Dictionary<ID, Texture>();

	public static Dictionary<ID, UseItemDelegate> UseDelegates = new Dictionary<ID, UseItemDelegate>();

	public static Dictionary<ID, IdInfo> IdInfos = new Dictionary<ID, IdInfo>();

	public static Shader TileShader { get; private set; }

	Items()
	{
		if(Engine.EditorHint) {return;}

		TileShader = GD.Load<Shader>("res://World/Materials/TileShader.shader");

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


	public static Vector3? TryCalculateBuildPosition(ID Branch, Tile Base, float PlayerOrientation, int BuildRotation, Vector3 Hit)
	{
		BuildInfoDelegate Function = IdInfos[Branch].PositionDelegate;

		if(Function != null)
		{
			Vector3? PossiblePosition = Function(Base, PlayerOrientation, BuildRotation, Hit - Base.Translation);
			if(PossiblePosition is Vector3 Position) //For now round all positions until it causes issues
				return new Vector3(Round(Position.x), Round(Position.y), Round(Position.z));
		}

		return null;
	}


	public static Vector3 CalculateBuildRotation(ID Branch, Tile Base, float PlayerOrientation, int BuildRotation, Vector3 Hit) //Always return a valid rotation
	{
		BuildInfoDelegate Function = IdInfos[Branch].RotationDelegate;

		if(Function != null)
		{
			Vector3? PossibleRotation = Function(Base, PlayerOrientation, BuildRotation, Hit - Base.Translation);
			if(PossibleRotation is Vector3 Rotation) //For now round all rotations until it causes issues
				return new Vector3(Round(Rotation.x), Round(Rotation.y), Round(Rotation.z));
		}

		return new Vector3();
	}


	public static void UseItem(Instance Item, Player UsingPlayer)
	{
		UseItemDelegate PossibleFunc = IdInfos[Item.Id].UseDelegate;
		if(PossibleFunc is UseItemDelegate Func)
		{
			Func(Item, UsingPlayer);
		}
	}


	public static void SetupItems()
	{
		IdInfos = new Dictionary<ID, IdInfo>() {
			{
				ID.ERROR,

				new IdInfo{}
			},

			{
				ID.PLATFORM,

				new IdInfo {
					PositionDelegate = BuildingLogic.PlatformBuildPosition,
					RotationDelegate = BuildingLogic.PlatformBuildRotation,
					UseDelegate = null,
					CanAds = false,
					DisallowedCollisions = new ID[] {ID.PLATFORM}
				}
			},

			{
				ID.WALL,

				new IdInfo {
					PositionDelegate = BuildingLogic.WallBuildPosition,
					RotationDelegate = BuildingLogic.WallBuildRotation,
					UseDelegate = null,
					CanAds = false,
					DisallowedCollisions = new ID[] {ID.WALL, ID.TRIANGLE_WALL}
				}
			},

			{
				ID.SLOPE,

				new IdInfo {
					PositionDelegate = BuildingLogic.SlopeBuildPosition,
					RotationDelegate = BuildingLogic.SlopeBuildRotation,
					UseDelegate = null,
					CanAds = false,
					DisallowedCollisions = new ID[] {ID.SLOPE}
				}
			},

			{
				ID.TRIANGLE_WALL,

				new IdInfo {
					PositionDelegate = BuildingLogic.TriangleWallBuildPosition,
					RotationDelegate = BuildingLogic.TriangleWallBuildRotation,
					UseDelegate = null,
					CanAds = false,
					DisallowedCollisions = new ID[] {ID.TRIANGLE_WALL, ID.WALL}
				}
			},

			{
				ID.ROCKET_JUMPER,

				new IdInfo {
					UseDelegate = RocketJumper.Fire,
					CanAds = false
				}
			},

			{
				ID.THUNDERBOLT,

				new IdInfo {
					UseDelegate = Thunderbolt.Fire,
					CanAds = true
				}
			}
		};

		//Lets make sure that every ID has an entry
		//This won't help mods but will help us greatly
		foreach(ID Type in System.Enum.GetValues(typeof(ID)))
		{
			Assert(IdInfos.ContainsKey(Type));
		}
	}
}
