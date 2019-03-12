using Godot;
using static System.Math;
using static SteelMath;


public class BuildPositions
{
	private static System.Nullable<Vector3> PlatformBranch(Structure Base)
	{
		switch(Base.Type)
		{
			case(Items.TYPE.PLATFORM):{
				float Rotation = Mathf.Deg2Rad(SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4));
				Vector3 Position = Base.Translation + (new Vector3(0,0,12)).Rotated(new Vector3(0,1,0), Rotation);
				return new Vector3(Mathf.Round(Position.x), Mathf.Round(Position.y), Mathf.Round(Position.z));
			}

			case(Items.TYPE.WALL):{
				float RotationDegrees = LoopRotation(SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4) + 180);

				if(RotationDegrees != LoopRotation((float)Round(Base.RotationDegrees.y)) && LoopRotation(RotationDegrees+180) != LoopRotation((float)Round(Base.RotationDegrees.y)))
				{
					return null;
				}

				Vector3 Position = Base.Translation + (new Vector3(0,6,6)).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(RotationDegrees));
				return new Vector3(Mathf.Round(Position.x), Mathf.Round(Position.y), Mathf.Round(Position.z));
			}

			case(Items.TYPE.SLOPE):{
				float RotationDegrees = LoopRotation(SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4));

				if(RotationDegrees != LoopRotation((float)Round(Base.RotationDegrees.y)) && LoopRotation(RotationDegrees+180) != LoopRotation((float)Round(Base.RotationDegrees.y)))
				{
					return null;
				}

				Vector3 Position;
				if(RotationDegrees == LoopRotation((float)Round(Base.RotationDegrees.y)))
				{
					Position = Base.Translation + (new Vector3(0,6,12)).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(RotationDegrees));
				}
				else
				{
					Position = Base.Translation + (new Vector3(0,-6,12)).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(RotationDegrees));
				}
				return new Vector3(Mathf.Round(Position.x), Mathf.Round(Position.y), Mathf.Round(Position.z));
			}

			default:
				return null;
		}
	}


	private static System.Nullable<Vector3> WallBranch(Structure Base)
	{
		switch(Base.Type)
		{
			case(Items.TYPE.PLATFORM):
			{
				float Rotation = Mathf.Deg2Rad(SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4));
				Vector3 Position = Base.Translation + (new Vector3(0,6,6)).Rotated(new Vector3(0,1,0), Rotation);
				return new Vector3(Mathf.Round(Position.x), Mathf.Round(Position.y), Mathf.Round(Position.z));
			}

			case(Items.TYPE.SLOPE):{
				float RotationDegrees = LoopRotation(SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4));

				if(RotationDegrees != LoopRotation((float)Round(Base.RotationDegrees.y)) && LoopRotation(RotationDegrees+180) != LoopRotation((float)Round(Base.RotationDegrees.y)))
				{
					return null;
				}

				Vector3 Position;
				if(RotationDegrees == LoopRotation((float)Round(Base.RotationDegrees.y)))
				{
					Position = Base.Translation + (new Vector3(0,12,6)).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(RotationDegrees));
				}
				else
				{
					Position = Base.Translation + (new Vector3(0,0,6)).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(RotationDegrees));
				}
				return new Vector3(Mathf.Round(Position.x), Mathf.Round(Position.y), Mathf.Round(Position.z));
			}

			case(Items.TYPE.WALL):{
				return Base.Translation + new Vector3(0,12,0);
			}

			default:
				return null;
		}
	}


	private static System.Nullable<Vector3> SlopeBranch(Structure Base)
	{
		switch(Base.Type)
		{
			case(Items.TYPE.PLATFORM):
			{
				float Rotation = Mathf.Deg2Rad(SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4));

				Vector3 Position;
				if(Game.PossessedPlayer.BuildRotation == 0)
				{
					Position = Base.Translation + (new Vector3(0,6,12)).Rotated(new Vector3(0,1,0), Rotation);
				}
				else
				{
					Position = Base.Translation + (new Vector3(0,-6,12)).Rotated(new Vector3(0,1,0), Rotation);
				}

				return new Vector3(Mathf.Round(Position.x), Mathf.Round(Position.y), Mathf.Round(Position.z));
			}

			case(Items.TYPE.SLOPE):{
				float RotationDegrees = LoopRotation(SnapToGrid(Game.PossessedPlayer.RotationDegrees.y, 360, 4));

				if(RotationDegrees != LoopRotation((float)Round(Base.RotationDegrees.y)) && LoopRotation(RotationDegrees+180) != LoopRotation((float)Round(Base.RotationDegrees.y)))
				{
					return null;
				}

				Vector3 Position;
				if(RotationDegrees == LoopRotation((float)Round(Base.RotationDegrees.y)))
				{
					if(Game.PossessedPlayer.BuildRotation == 0)
					{
						Position = Base.Translation + (new Vector3(0,12,12)).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(RotationDegrees));
					}
					else
					{
						Position = Base.Translation + (new Vector3(0,0,12)).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(RotationDegrees));
					}

				}
				else
				{
					if(Game.PossessedPlayer.BuildRotation == 0)
					{
						Position = Base.Translation + (new Vector3(0,0,12)).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(RotationDegrees));
					}
					else
					{
						Position = Base.Translation + (new Vector3(0,-12,12)).Rotated(new Vector3(0,1,0), Mathf.Deg2Rad(RotationDegrees));
					}
				}
				return new Vector3(Mathf.Round(Position.x), Mathf.Round(Position.y), Mathf.Round(Position.z));
			}

			default:
				return null;
		}
	}


	public static System.Nullable<Vector3> Calculate(Structure Base, Items.TYPE BranchType)
	{
		switch(BranchType)
		{
			case(Items.TYPE.PLATFORM):
				return PlatformBranch(Base);

			case(Items.TYPE.WALL):
				return WallBranch(Base);

			case(Items.TYPE.SLOPE):
				return SlopeBranch(Base);

			default:
				//Return null if unsuported, will be caught by Building.Request
				return null;
		}
	}
}
