

using UnrealBuildTool;
using System.Collections.Generic;

public class SkyOfSteelTarget : TargetRules
{
	public SkyOfSteelTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Game;

		ExtraModuleNames.AddRange( new string[] { "SkyOfSteel" } );
	}
}
