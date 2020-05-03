using Godot;
using static Godot.Mathf;
using static SteelMath;


public class BuildingLogic {
	public static Vector3? PlatformBuildPosition(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		switch(Base.ItemId) {
			case (Items.ID.PLATFORM): {
					PlayerOrientation = LoopRotation(Deg2Rad(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)));
					return Base.Translation + (new Vector3(0, 0, 12)).Rotated(new Vector3(0, 1, 0), PlayerOrientation);
				}

			case (Items.ID.WALL): {
					float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4) + 180);

					if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
					   && LoopRotation(Orientation + 180) != LoopRotation(Round(Base.RotationDegrees.y))) {
						return null;
					}

					if(BuildRotation == 1 || BuildRotation == 3)
						Orientation = LoopRotation(Orientation + 180);

					int yOffset = 6;
					if(HitRelative.y + Base.Translation.y < Base.Translation.y)
						yOffset = -6;

					return new Vector3(0, yOffset, 6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
				}

			case (Items.ID.SLOPE): {
					float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

					if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
					   && LoopRotation(Orientation + 180) != LoopRotation(Round(Base.RotationDegrees.y))) {
						return null;
					}

					int zOffset = 12;
					if(BuildRotation == 1 || BuildRotation == 3)
						zOffset = 0;

					if(Orientation == LoopRotation(Round(Base.RotationDegrees.y))) {
						return new Vector3(0, 6, zOffset).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
					}
					else {
						return new Vector3(0, -6, zOffset).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
					}
				}
		}

		return null;
	}


	public static Vector3? PlatformBuildRotation(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		return new Vector3(); //PLATFORM will always have a rotation of 0,0,0
	}




	public static Vector3? WallBuildPosition(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		switch(Base.ItemId) {
			case (Items.ID.PLATFORM): {
					PlayerOrientation = LoopRotation(Deg2Rad(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)));

					int yOffset = 6;
					if(BuildRotation == 1 || BuildRotation == 3)
						yOffset = -6;

					return new Vector3(0, yOffset, 6).Rotated(new Vector3(0, 1, 0), PlayerOrientation) + Base.Translation;
				}

			case (Items.ID.WALL): {
					float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

					if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
					   && LoopRotation(Orientation + 180) != LoopRotation(Round(Base.RotationDegrees.y))) //Not facing straight on
					{
						return new Vector3(0, 0, 12).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
					}
					else {
						if(HitRelative.y + Base.Translation.y >= Base.Translation.y)
							return new Vector3(0, 12, 0) + Base.Translation;
						else
							return new Vector3(0, -12, 0) + Base.Translation;
					}
				}

			case (Items.ID.SLOPE): {
					float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

					if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
					   && LoopRotation(Orientation + 180) != LoopRotation(Round(Base.RotationDegrees.y))) {
						return null;
					}

					int yOffset = 0;
					if(BuildRotation == 1 || BuildRotation == 3)
						yOffset = -12;

					if(Orientation == LoopRotation(Round(Base.RotationDegrees.y))) {
						return new Vector3(0, 12 + yOffset, 6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
					}
					else {
						return new Vector3(0, yOffset, 6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
					}
				}
		}

		return null;
	}


	public static Vector3? WallBuildRotation(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		if(Base.ItemId == Items.ID.WALL || Base.ItemId == Items.ID.SLOPE)
			return Base.RotationDegrees;

		return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)), 0);

	}




	public static Vector3? SlopeBuildPosition(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		switch(Base.ItemId) {
			case (Items.ID.PLATFORM): {
					float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

					int yOffset = 6;
					if(BuildRotation == 1 || BuildRotation == 3)
						yOffset = -6;

					return new Vector3(0, yOffset, 12).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
				}

			case (Items.ID.WALL): {
					float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));
					if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
					   && LoopRotation(Orientation + 180) != LoopRotation(Round(Base.RotationDegrees.y))) {
						return null;
					}

					if(HitRelative.y + Base.Translation.y >= Base.Translation.y) {
						if(BuildRotation == 0)
							return new Vector3(0, 12, 6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
						if(BuildRotation == 1)
							return new Vector3(0, 0, 6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
						if(BuildRotation == 2)
							return new Vector3(0, 12, -6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
						if(BuildRotation == 3)
							return new Vector3(0, 0, -6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
					}
					else {
						if(BuildRotation == 0)
							return new Vector3(0, 0, 6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
						if(BuildRotation == 1)
							return new Vector3(0, -12, 6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
						if(BuildRotation == 2)
							return new Vector3(0, 0, -6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
						if(BuildRotation == 3)
							return new Vector3(0, -12, -6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
					}

					return null;
				}

			case (Items.ID.SLOPE): {
					float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));
					if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
					   && LoopRotation(Orientation + 180) != LoopRotation(Round(Base.RotationDegrees.y))) {
						return new Vector3(0, 0, 12).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
					}

					if(Orientation == LoopRotation(Round(Base.RotationDegrees.y))) {
						if(BuildRotation == 0 || BuildRotation == 2) {
							return new Vector3(0, 12, 12).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
						}
						else {
							return new Vector3(0, 0, 12).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
						}

					}
					else {
						if(BuildRotation == 0 || BuildRotation == 2) {
							return new Vector3(0, 0, 12).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
						}
						else {
							return new Vector3(0, -12, 12).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
						}
					}
				}
		}

		return null;
	}


	public static Vector3? SlopeBuildRotation(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		if(Base.ItemId == Items.ID.PLATFORM)
			if(BuildRotation == 1 || BuildRotation == 3)
				return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)) + 180, 0);

		if(Base.ItemId == Items.ID.WALL) {
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

		if(Base.ItemId == Items.ID.SLOPE) {
			if(LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)) == LoopRotation(Round(Base.RotationDegrees.y))) {
				if(BuildRotation == 0 || BuildRotation == 2)
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Base.RotationDegrees.y), 360, 4)), 0);
				if(BuildRotation == 1 || BuildRotation == 3)
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Base.RotationDegrees.y + 180), 360, 4)), 0);
			}
			else {
				if(BuildRotation == 0 || BuildRotation == 2)
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Base.RotationDegrees.y + 180), 360, 4)), 0);
				if(BuildRotation == 1 || BuildRotation == 3)
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(Base.RotationDegrees.y), 360, 4)), 0);
			}
		}

		return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)), 0);
	}




	public static Vector3? TriangleWallBuildPosition(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		switch(Base.ItemId) {
			case (Items.ID.PLATFORM): {
					PlayerOrientation = LoopRotation(Deg2Rad(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)));

					int yOffset = 6;
					if(BuildRotation == 2 || BuildRotation == 3)
						yOffset = -6;

					return new Vector3(0, yOffset, 6).Rotated(new Vector3(0, 1, 0), PlayerOrientation) + Base.Translation;
				}
			case (Items.ID.WALL): {
					float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

					if(HitRelative.y + Base.Translation.y >= Base.Translation.y)
						return new Vector3(0, 12, 0) + Base.Translation;
					else
						return new Vector3(0, -12, 0) + Base.Translation;
				}
			case (Items.ID.SLOPE): {
					float Orientation = LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4));

					if(Orientation != LoopRotation(Round(Base.RotationDegrees.y))
					   && LoopRotation(Orientation + 180) != LoopRotation(Round(Base.RotationDegrees.y))) {
						return null;
					}

					int yOffset = 0;
					if(BuildRotation == 2 || BuildRotation == 3)
						yOffset = -12;

					if(Orientation == LoopRotation(Round(Base.RotationDegrees.y))) {
						return new Vector3(0, 12 + yOffset, 6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
					}
					else {
						return new Vector3(0, yOffset, 6).Rotated(new Vector3(0, 1, 0), Deg2Rad(Orientation)) + Base.Translation;
					}
				}
		}

		return null;
	}


	public static Vector3? TriangleWallBuildRotation(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		if(Base.ItemId == Items.ID.PLATFORM || Base.ItemId == Items.ID.SLOPE) {
			if(BuildRotation == 1)
				return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation + 180), 360, 4)), 0);

			else if(BuildRotation == 2)
				return new Vector3(180, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)), 0);

			else if(BuildRotation == 3)
				return new Vector3(180, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation + 180), 360, 4)), 0);
		}

		else if(Base.ItemId == Items.ID.WALL) {
			float yRot = Base.RotationDegrees.y;
			if(HitRelative.y + Base.Translation.y >= Base.Translation.y) {
				if(BuildRotation == 1 || BuildRotation == 3)
					return new Vector3(0, LoopRotation(yRot + 180), 0);
				return new Vector3(0, LoopRotation(yRot), 0);
			}
			else {
				if(BuildRotation == 1 || BuildRotation == 3)
					return new Vector3(180, LoopRotation(yRot + 180), 0);
				return new Vector3(180, LoopRotation(yRot), 0);
			}
		}

		return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)), 0);
	}


	public static Vector3? PipeBuildPosition(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		switch(Base.ItemId) {
			case (Items.ID.PLATFORM): {
					return Base.Translation + new Vector3(0, 6, 0);
				}

			case (Items.ID.PIPE): {
					{
						Vector3 First = RoundVec3(Base.GetNode<Spatial>("Positions/Position1").GlobalTransform.origin);
						Vector3 Second = RoundVec3(Base.GetNode<Spatial>("Positions/Position2").GlobalTransform.origin);
						if(First.y != Second.y) {
							if(HitRelative.y > 0)
								return Base.Translation + new Vector3(0, 12, 0);
							return Base.Translation - new Vector3(0, 12, 0);
						}
					}

					if(LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)) != LoopRotation(Round(Base.RotationDegrees.y))
					   && LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)) != LoopRotation(Round(Base.RotationDegrees.y) + 180))
						return null;

					Vector3 Offset = new Vector3(0, 0, 12).Rotated(new Vector3(0, 1, 0), Deg2Rad(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)));
					return Base.Translation + Offset;
				}

			case (Items.ID.PIPE_JOINT): {
					float xAbs = Abs(HitRelative.x);
					float yAbs = Abs(HitRelative.y);
					float zAbs = Abs(HitRelative.z);

					if(xAbs > yAbs && xAbs > zAbs) {
						//X axis
						return Base.Translation + (new Vector3(12, 0, 0) * SafeSign(HitRelative.x));
					}
					else if(yAbs > xAbs && yAbs > zAbs) {
						//Y axis
						return Base.Translation + (new Vector3(0, 12, 0) * SafeSign(HitRelative.y));
					}
					else if(zAbs > xAbs && zAbs > yAbs) {
						//Z axis
						return Base.Translation + (new Vector3(0, 0, 12) * SafeSign(HitRelative.z));
					}

					break;
				}
		}

		return null;
	}


	public static Vector3? PipeBuildRotation(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		switch(Base.ItemId) {
			case (Items.ID.PLATFORM): {
					return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)), 0);
				}

			case (Items.ID.PIPE): {
					return Base.RotationDegrees;
				}

			case (Items.ID.PIPE_JOINT): {
					float xAbs = Abs(HitRelative.x);
					float yAbs = Abs(HitRelative.y);
					float zAbs = Abs(HitRelative.z);

					if(xAbs > yAbs && xAbs > zAbs) {
						//X axis
						return new Vector3(0, 90, 0);
					}
					else if(yAbs > xAbs && yAbs > zAbs) {
						//Y axis
						return new Vector3(90, 0, 0);
					}
					else if(zAbs > xAbs && zAbs > yAbs) {
						//Z axis
						return new Vector3(0, 0, 0);
					}

					break;
				}
		}

		return null;
	}


	public static Vector3? PipeJointBuildPosition(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		switch(Base.ItemId) {
			case (Items.ID.PLATFORM): {
					return Base.Translation + new Vector3(0, 6, 0);
				}

			case (Items.ID.PIPE): {
					{
						Vector3 First = RoundVec3(Base.GetNode<Spatial>("Positions/Position1").GlobalTransform.origin);
						Vector3 Second = RoundVec3(Base.GetNode<Spatial>("Positions/Position2").GlobalTransform.origin);
						if(First.y != Second.y) {
							if(HitRelative.y > 0)
								return Base.Translation + new Vector3(0, 12, 0);
							return Base.Translation - new Vector3(0, 12, 0);
						}
					}

					if(LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)) != LoopRotation(Round(Base.RotationDegrees.y))
					   && LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)) != LoopRotation(Round(Base.RotationDegrees.y) + 180))
						return null;

					Vector3 Offset = new Vector3(0, 0, 12).Rotated(new Vector3(0, 1, 0), Deg2Rad(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)));
					return Base.Translation + Offset;
				}

			case (Items.ID.PIPE_JOINT): {
					float xAbs = Abs(HitRelative.x);
					float yAbs = Abs(HitRelative.y);
					float zAbs = Abs(HitRelative.z);

					if(xAbs > yAbs && xAbs > zAbs) {
						//X axis
						return Base.Translation + (new Vector3(12, 0, 0) * SafeSign(HitRelative.x));
					}
					else if(yAbs > xAbs && yAbs > zAbs) {
						//Y axis
						return Base.Translation + (new Vector3(0, 12, 0) * SafeSign(HitRelative.y));
					}
					else if(zAbs > xAbs && zAbs > yAbs) {
						//Z axis
						return Base.Translation + (new Vector3(0, 0, 12) * SafeSign(HitRelative.z));
					}

					break;
				}
		}

		return null;
	}


	public static Vector3? PipeJointBuildRotation(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		return new Vector3(); //Always 0,0,0
	}



	public static Vector3? LockerBuildPosition(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		if(Base.ItemId == Items.ID.PLATFORM) {
			return new Vector3(0, 6, 0) + Base.Translation;
		}

		return null;
	}


	public static Vector3? LockerBuildRotation(Tile Base, float PlayerOrientation, int BuildRotation, Vector3 HitRelative) {
		if(Base.ItemId == Items.ID.WALL || Base.ItemId == Items.ID.SLOPE)
			return Base.RotationDegrees;

		return new Vector3(0, LoopRotation(SnapToGrid(LoopRotation(PlayerOrientation), 360, 4)), 0);

	}
}
