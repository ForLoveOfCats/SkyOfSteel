using Godot;
using System;


public class DroppedItem : KinematicBody
{
	public Items.TYPE Type = Items.TYPE.ERROR;

	public override void _Ready()
	{
		ShaderMaterial Mat = new ShaderMaterial();
		Mat.Shader = Items.StructureShader;
		Mat.SetShaderParam("texture_albedo", Items.Textures[Type]);
		GetNode<MeshInstance>("MeshInstance").MaterialOverride = Mat;
	}
}
