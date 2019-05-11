using Godot;
using System;
using System.Collections.Generic;


public class Items : Node
{
	public class Instance
	{
		public Items.TYPE Type = Items.TYPE.ERROR;
		public int Temperature = 0;
		public int Count = 1;
		public int UsesRemaining = 0;

		public Instance(Items.TYPE TypeArg)
		{
			this.Type = TypeArg;
		}
	}


	private struct CustomItemEnum //Reference implimentation for 0.1.3
	{
		public static int NextSpot = Enum.GetNames(typeof(TYPE)).Length;
		public int Spot;

		public CustomItemEnum(int SpotArg)
		{
			Spot = SpotArg;
		}


		public static implicit operator TYPE(CustomItemEnum ItemEnum)
		{
			return (TYPE)(ItemEnum.Spot);
		}
	}

	private static CustomItemEnum NewCustomItemEnum() //Reference implimentation for 0.1.3
	{
		CustomItemEnum NewItemEnum = new CustomItemEnum(CustomItemEnum.NextSpot);
		CustomItemEnum.NextSpot++;
		return NewItemEnum;
	}


	public enum TYPE {ERROR, PLATFORM, WALL, SLOPE}

	public static Dictionary<TYPE, Mesh> Meshes = new Dictionary<TYPE, Mesh>();
	public static Dictionary<TYPE, Texture> Thumbnails = new Dictionary<TYPE, Texture>();
	public static Dictionary<TYPE, Texture> Textures { get; private set; } = new Dictionary<TYPE, Texture>();

	public static Shader StructureShader { get; private set; }

	Items()
	{
		if(Engine.EditorHint) {return;}

		StructureShader = GD.Load<Shader>("res://World/Materials/StructureShader.shader");

		foreach(Items.TYPE Type in System.Enum.GetValues(typeof(TYPE)))
		{
			Meshes.Add(Type, GD.Load<Mesh>($"res://Items/Meshes/{Type}.obj"));
			//Assume that every item has a mesh, will throw exception on game startup if not
		}

		foreach(TYPE Type in System.Enum.GetValues(typeof(TYPE)))
		{
			Thumbnails.Add(Type, GD.Load<Texture>($"res://Items/Thumbnails/{Type}.png"));
			//Assume that every item has a thumbnail, will throw exception on game startup if not
		}

		foreach(TYPE Type in System.Enum.GetValues(typeof(TYPE)))
		{
			Textures.Add(Type, GD.Load<Texture>($"res://Items/Textures/{Type}.png"));
			//Assume that every item has a texture, will throw exception on game startup if not
		}
	}
}
