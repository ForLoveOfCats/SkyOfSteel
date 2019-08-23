using Godot;
using static Godot.Mathf;
using static SteelMath;


public class BuildingLogic
{
	public static Vector3? PlatformBuildPosition(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative)
	{
		switch(Base.Type)
		{
			case(Items.ID.PLATFORM):
			{
				PlayerOrientation = LoopRotation(Deg2Rad(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)));
				return Base.Translation + (new Vector3(0,0,12)).Rotated(new Vector3(0,1,0), PlayerOrientation);
			}

			case(Items.ID.WALL):
			{
				float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4) + 180);

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

				return new Vector3(0, yOffset, 6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
			}

			case(Items.ID.SLOPE):
			{
				float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

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
					return new Vector3(0, 6, zOffset).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
				}
				else
				{
					return new Vector3(0, -6, zOffset).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
				}
			}

			case(Items.ID.TRIANGLE_PLATFORM):
			{
				float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));
				if(LoopRotation(Orientation) == LoopRotation(Round(Base.RotationDegrees.y) + 180))
				{
					return Base.Translation + (new Vector3(0,0,12)).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation));
				}

				Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation - Base.RotationDegrees.y  - 90), 360, 2) + Round(Base.RotationDegrees.y) + 90);
				return Base.Translation + (new Vector3(0,0,12)).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation));
			}
		}

		return null;
	}


	public static Vector3? PlatformBuildRotation(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative)
	{
		if(Base.Type == Items.ID.TRIANGLE_PLATFORM)
		{
			float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation - Base.RotationDegrees.y  - 90), 360, 2));
			if(Orientation == 180)
				return new Vector3(0, LoopRotation(Round(Base.RotationDegrees.y) + 45), 0);
			else if(Orientation == 0)
				return new Vector3(0, LoopRotation(Round(Base.RotationDegrees.y) - 45), 0);
		}

		return new Vector3();
	}




	public static Vector3? WallBuildPosition(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative)
	{
		switch(Base.Type)
		{
			case(Items.ID.PLATFORM):
			{
				PlayerOrientation = LoopRotation(Deg2Rad(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)));

				int yOffset = 6;
				if(BuildRotation == 1 || BuildRotation == 3)
					yOffset = -6;

				return new Vector3(0, yOffset, 6).Rotated(new Vector3(0,1,0), PlayerOrientation) + Base.Translation;
			}

			case(Items.ID.WALL):
			{
				float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

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

			case(Items.ID.SLOPE):
			{
				float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

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
					return new Vector3(0, 12 + yOffset, 6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
				}
				else
				{
					return new Vector3(0, yOffset, 6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
				}
			}
		}

		return null;
	}


	public static Vector3? WallBuildRotation(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative)
	{
		if(Base.Type == Items.ID.WALL || Base.Type == Items.ID.SLOPE)
			return Base.RotationDegrees;

		return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)), 0);
	}




	public static Vector3? SlopeBuildPosition(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative)
	{
		switch(Base.Type)
		{
			case(Items.ID.PLATFORM):
			{
				float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

				int yOffset = 6;
				if(BuildRotation == 1 || BuildRotation == 3)
					yOffset = -6;

				return new Vector3(0, yOffset, 12).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
			}

			case(Items.ID.WALL):
			{
				float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));
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

			case(Items.ID.SLOPE):
			{
				float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));
				if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
				   && LoopRotation(Orientation+180) != LoopRotation(Round(Base.RotationDegrees.y)))
				{
					return new Vector3(0, 0, 12).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
				}

				if(Orientation == LoopRotation(Round(Base.RotationDegrees.y)))
				{
					if(BuildRotation == 0 || BuildRotation == 2)
					{
						return new Vector3(0,12,12).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
					}
					else
					{
						return new Vector3(0,0,12).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
					}

				}
				else
				{
					if(BuildRotation == 0 || BuildRotation == 2)
					{
						return new Vector3(0,0,12).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
					}
					else
					{
						return new Vector3(0,-12,12).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
					}
				}
			}
		}

		return null;
	}


	public static Vector3? SlopeBuildRotation(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative)
	{
		if(Base.Type == Items.ID.PLATFORM)
			if(BuildRotation == 1 || BuildRotation == 3)
				return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)) + 180, 0);

		if(Base.Type == Items.ID.WALL)
		{
			float Orientation = LoopRotation(SnapToGrid(PlayerOrientation, 360, 4));
			if(BuildRotation == 0)
				return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Orientation), 360, 4)), 0);
			if(BuildRotation == 1)
				return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Orientation + 180), 360, 4)), 0);
			if(BuildRotation == 2)
				return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Orientation + 180), 360, 4)), 0);
			if(BuildRotation == 3)
				return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Orientation), 360, 4)), 0);
		}

		if(Base.Type == Items.ID.SLOPE)
		{
			if(LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)) == LoopRotation(Round(Base.RotationDegrees.y)))
			{
				if(BuildRotation == 0 || BuildRotation == 2)
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Base.RotationDegrees.y), 360, 4)), 0);
				if(BuildRotation == 1 || BuildRotation == 3)
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Base.RotationDegrees.y + 180), 360, 4)), 0);
			}
			else
			{
				if(BuildRotation == 0 || BuildRotation == 2)
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Base.RotationDegrees.y + 180), 360, 4)), 0);
				if(BuildRotation == 1 || BuildRotation == 3)
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Base.RotationDegrees.y), 360, 4)), 0);
			}
		}

		return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)), 0);
	}




	public static Vector3? TriangleWallBuildPosition(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative)
	{
		switch(Base.Type)
		{
			case(Items.ID.PLATFORM):
			{
				PlayerOrientation = LoopRotation(Deg2Rad(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)));

				int yOffset = 6;
				if(BuildRotation == 2 || BuildRotation == 3)
					yOffset = -6;

				return new Vector3(0, yOffset, 6).Rotated(new Vector3(0,1,0), PlayerOrientation) + Base.Translation;
			}
			case(Items.ID.WALL):
			{
				float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

				if(HitRelative.y + Base.Translation.y >= Base.Translation.y)
					return new Vector3(0, 12, 0) + Base.Translation;
				else
					return new Vector3(0, -12, 0) + Base.Translation;
			}
			case(Items.ID.SLOPE):
			{
				float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

				if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
				   && LoopRotation(Orientation+180) != LoopRotation(Round(Base.RotationDegrees.y)))
				{
					return null;
				}

				int yOffset = 0;
				if(BuildRotation == 2 || BuildRotation == 3)
					yOffset = -12;

				if(Orientation == LoopRotation(Round(Base.RotationDegrees.y)))
				{
					return new Vector3(0, 12 + yOffset, 6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
				}
				else
				{
					return new Vector3(0, yOffset, 6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
				}
			}
		}

		return null;
	}


	public static Vector3? TriangleWallBuildRotation(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative)
	{
		if(Base.Type == Items.ID.PLATFORM || Base.Type == Items.ID.SLOPE)
		{
			if(BuildRotation == 1)
				return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation + 180), 360, 4)), 0);

			else if(BuildRotation == 2)
				return new Vector3(180, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)), 0);

			else if(BuildRotation == 3)
				return new Vector3(180, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation + 180), 360, 4)), 0);
		}

		else if(Base.Type == Items.ID.WALL)
		{
			float yRot = Base.RotationDegrees.y;
			if(HitRelative.y + Base.Translation.y >= Base.Translation.y)
			{
				if(BuildRotation == 1 || BuildRotation == 3)
					return new Vector3(0, LoopRotation(yRot + 180), 0);
				return new Vector3(0, LoopRotation(yRot), 0);
			}
			else
			{
				if(BuildRotation == 1 || BuildRotation == 3)
					return new Vector3(180, LoopRotation(yRot + 180), 0);
				return new Vector3(180, LoopRotation(yRot), 0);
			}
		}

		return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)), 0);
	}


	public static Vector3? TrianglePlatformBuildPosition(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative)
	{
		switch(Base.Type)
		{
			case(Items.ID.PLATFORM):
			{
				PlayerOrientation = LoopRotation(Deg2Rad(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)));
				return Base.Translation + (new Vector3(0,0,12)).Rotated(new Vector3(0,1,0), PlayerOrientation);
			}

			case(Items.ID.WALL):
			{
				float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4) + 180);

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

				return new Vector3(0, yOffset, 6).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
			}

			case(Items.ID.SLOPE):
			{
				float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

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
					return new Vector3(0, 6, zOffset).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
				}
				else
				{
					return new Vector3(0, -6, zOffset).Rotated(new Vector3(0,1,0), Deg2Rad(Orientation)) + Base.Translation;
				}
			}
		}

		return null;
	}


	public static Vector3? TrianglePlatformBuildRotation(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative)
	{
		if(Base.Type == Items.ID.WALL)
		{
			float Orientation = LoopRotation(SnapToGrid(PlayerOrientation, 360, 4));
			if(BuildRotation == 0)
				return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Orientation + 180), 360, 4)), 0);
			if(BuildRotation == 1)
				return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Orientation), 360, 4)), 0);
			if(BuildRotation == 2)
				return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Orientation + 180), 360, 4)), 0);
			if(BuildRotation == 3)
				return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Orientation), 360, 4)), 0);
		}

		if(Base.Type == Items.ID.SLOPE)
		{
			if(LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)) == LoopRotation(Round(Base.RotationDegrees.y)))
			{
				if(BuildRotation == 0 || BuildRotation == 2)
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Base.RotationDegrees.y), 360, 4)), 0);
				if(BuildRotation == 1 || BuildRotation == 3)
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Base.RotationDegrees.y + 180), 360, 4)), 0);
			}
			else
			{
				if(BuildRotation == 0 || BuildRotation == 2)
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Base.RotationDegrees.y + 180), 360, 4)), 0);
				if(BuildRotation == 1 || BuildRotation == 3)
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Base.RotationDegrees.y), 360, 4)), 0);
			}
		}

		return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)), 0);
	}
}
