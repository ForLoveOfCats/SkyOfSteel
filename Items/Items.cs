using Godot;
using System;
using System.Collections.Generic;


public class Items : Node
{
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
}
