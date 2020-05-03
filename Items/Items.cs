using Godot;
using Optional;
using System;
using System.Collections.Generic;
using static Godot.Mathf;



public class Items : Node {
	public delegate Vector3? BuildInfoDelegate(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative);
	public delegate void UseItemDelegate(Instance Item, Player UsingPlayer);


	public class Instance {
		[Newtonsoft.Json.JsonProperty("I")]
		public Items.ID Id = Items.ID.ERROR;
		[Newtonsoft.Json.JsonProperty("C")]
		public int Count = 1;

		public Instance(Items.ID IdArg) {
			Id = IdArg;
		}
	}


	public struct IdInfo {
		public BuildInfoDelegate PositionDelegate;
		public BuildInfoDelegate RotationDelegate;

		public UseItemDelegate UseDelegate;
		public bool FullAuto;
		public bool CanAds;

		public ID[] DisallowedCollisions;
	}


	public enum ID {
		NONE = int.MinValue,
		ERROR = 0,
		PLATFORM,
		WALL,
		SLOPE,
		TRIANGLE_WALL,
		PIPE,
		PIPE_JOINT,
		LOCKER,
		ROCKET_JUMPER,
		THUNDERBOLT,
		SCATTERSHOCK,
		SWIFTSPARK,
		SLIME_SPAWNER
	}

	public enum IntentCount { ALL, SINGLE, HALF };

	public static Dictionary<ID, Mesh> Meshes = new Dictionary<ID, Mesh>();
	public static Dictionary<ID, Texture> Thumbnails = new Dictionary<ID, Texture>();
	public static Dictionary<ID, Texture> Textures { get; private set; } = new Dictionary<ID, Texture>();

	public static Dictionary<ID, IdInfo> IdInfos = new Dictionary<ID, IdInfo>();

	public static Shader TileShader { get; private set; }

	Items() {
		if(Engine.EditorHint) { return; }

		TileShader = GD.Load<Shader>("res://World/Materials/TileShader.shader");

		//Assume that every item has a mesh, thumbnail, and texture. Will throw exception on game startup if not
		foreach(Items.ID Type in System.Enum.GetValues(typeof(ID))) {
			if(Type == Items.ID.NONE) continue;

			Meshes.Add(Type, GD.Load<Mesh>($"res://Items/Meshes/{Type}.obj"));
			Thumbnails.Add(Type, GD.Load<Texture>($"res://Items/Thumbnails/{Type}.png"));
			Textures.Add(Type, GD.Load<Texture>($"res://Items/Textures/{Type}.png"));
		}
	}


	public static int CalcRetrieveCount(IntentCount CountMode, int Value) {
		switch(CountMode) {
			case IntentCount.ALL:
				//Keep original count as original
				break;
			case IntentCount.HALF:
				if(Value != 1)
					Value /= 2; //Relying on rounding down via truncation
				break;
			case IntentCount.SINGLE:
				Value = 1;
				break;
		}

		return Value;
	}


	public static Vector3? TryCalculateBuildPosition(ID Branch, Tile Base, float PlayerOrientation, int BuildRotation, Vector3 Hit) {
		BuildInfoDelegate Function = IdInfos[Branch].PositionDelegate;

		if(Function != null) {
			Vector3? PossiblePosition = Function(Base, PlayerOrientation, BuildRotation, Hit - Base.Translation);
			if(PossiblePosition is Vector3 Position) //For now round all positions until it causes issues
				return new Vector3(Round(Position.x), Round(Position.y), Round(Position.z));
		}

		return null;
	}


	public static Vector3 CalculateBuildRotation(ID Branch, Tile Base, float PlayerOrientation, int BuildRotation, Vector3 Hit) //Always return a valid rotation
	{
		BuildInfoDelegate Function = IdInfos[Branch].RotationDelegate;

		if(Function != null) {
			Vector3? PossibleRotation = Function(Base, PlayerOrientation, BuildRotation, Hit - Base.Translation);
			if(PossibleRotation is Vector3 Rotation) //For now round all rotations until it causes issues
				return new Vector3(Round(Rotation.x), Round(Rotation.y), Round(Rotation.z));
		}

		return new Vector3();
	}


	public static void UseItem(Instance Item, Player UsingPlayer) {
		UseItemDelegate PossibleFunc = IdInfos[Item.Id].UseDelegate;
		if(PossibleFunc is UseItemDelegate Func) {
			Func(Item, UsingPlayer);
		}
	}


	public static void SetupItems() {
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
					FullAuto = false,
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
					FullAuto = false,
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
					FullAuto = false,
					CanAds = false,
					DisallowedCollisions = new ID[] {ID.SLOPE, ID.PIPE, ID.PIPE_JOINT}
				}
			},

			{
				ID.TRIANGLE_WALL,

				new IdInfo {
					PositionDelegate = BuildingLogic.TriangleWallBuildPosition,
					RotationDelegate = BuildingLogic.TriangleWallBuildRotation,
					UseDelegate = null,
					FullAuto = false,
					CanAds = false,
					DisallowedCollisions = new ID[] {ID.TRIANGLE_WALL, ID.WALL}
				}
			},

			{
				ID.PIPE,

				new IdInfo {
					PositionDelegate = BuildingLogic.PipeBuildPosition,
					RotationDelegate = BuildingLogic.PipeBuildRotation,
					DisallowedCollisions = new ID[] {ID.SLOPE, ID.PIPE, ID.PIPE_JOINT}
				}
			},

			{
				ID.PIPE_JOINT,

				new IdInfo {
					PositionDelegate = BuildingLogic.PipeJointBuildPosition,
					RotationDelegate = BuildingLogic.PipeJointBuildRotation,
					DisallowedCollisions = new ID[] {ID.SLOPE, ID.PIPE, ID.PIPE_JOINT}
				}
			},

			{
				ID.LOCKER,

				new IdInfo {
					PositionDelegate = BuildingLogic.LockerBuildPosition,
					RotationDelegate = BuildingLogic.LockerBuildRotation,
					DisallowedCollisions = new ID[] {ID.LOCKER}
				}
			},

			{
				ID.ROCKET_JUMPER,

				new IdInfo {
					UseDelegate = RocketJumper.Fire,
					FullAuto = false,
					CanAds = false
				}
			},

			{
				ID.THUNDERBOLT,

				new IdInfo {
					UseDelegate = Thunderbolt.Fire,
					FullAuto = false,
					CanAds = true
				}
			},

			{
				ID.SCATTERSHOCK,

				new IdInfo {
					UseDelegate = Scattershock.Fire,
					FullAuto = false,
					CanAds = true
				}
			},

			{
				ID.SWIFTSPARK,

				new IdInfo {
					UseDelegate = SwiftSpark.Fire,
					FullAuto = true,
					CanAds = true
				}
			},

			{
				ID.SLIME_SPAWNER,

				new IdInfo {
					UseDelegate = (Items.Instance Item, Player UsingPlayer) => {
						Mobs.SpawnMob(Mobs.ID.Slime, UsingPlayer.Translation);
					},
					FullAuto = false,
					CanAds = false
				}
			}
		};

		//Lets make sure that every ID has an entry
		//This won't help mods but will help us greatly
		foreach(ID Type in System.Enum.GetValues(typeof(ID))) {
			if(Type == ID.NONE) continue;

			Assert.ActualAssert(IdInfos.ContainsKey(Type));
		}
	}
}
