using Godot;
using System.Collections.Generic;



public class ThumbnailRenderer : Node
{
	Viewport RenderViewport;
	MeshInstance ThumbnailTarget;
	ShaderMaterial Mat;

	float DelayRemaining = 2;
	int Index = 0;
	List<Items.ID> IdList = new List<Items.ID>();


	public override void _Ready()
	{
		RenderViewport = GetNode<Viewport>("Viewport");

		ThumbnailTarget = GetNode<MeshInstance>("MeshInstance");

		Mat = new ShaderMaterial();
		Mat.Shader = GD.Load<Shader>("res://World/Materials/TileShader.shader");

		foreach(Items.ID Type in System.Enum.GetValues(typeof(Items.ID)))
			if(Type != Items.ID.ERROR && Type != Items.ID.NONE)
				IdList.Add(Type);
	}


	public override void _Process(float Delta)
	{
		DelayRemaining -= Delta;
		if(DelayRemaining >= 0)
			return;

		if(Index > 0 && Index <= IdList.Count)
			RenderViewport.GetTexture().GetData().SavePng($"res://Items/Thumbnails/{IdList[Index - 1]}.png");

		if(Index >= IdList.Count)
		{
			Index += 1;
			return;
		}

		GD.Print($"Rendering thumbnail for item '{IdList[Index]}'");

		Mesh ItemMesh = GD.Load<Mesh>($"res://Items/Meshes/{IdList[Index]}.obj");
		ThumbnailTarget.Mesh = ItemMesh;

		Texture ItemTexture = GD.Load<Texture>($"res://Items/Textures/{IdList[Index]}.png");
		ThumbnailTarget.MaterialOverride = Mat;
		Mat.SetShaderParam("texture_albedo", ItemTexture);

		Index += 1;
	}
}
