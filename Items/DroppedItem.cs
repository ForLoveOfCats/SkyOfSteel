using Godot;
using System;


public class DroppedItem : KinematicBody, IInGrid, IPushable
{
	private const float Gravity = 14f;
	private const float MaxFallSpeed = -40f;
	private const float Friction = 15f;
	private const float RPS = 0.5f; //Revolutions Per Second
	public static float MinPickupLife = 0.6f; //In seconds

	public System.Tuple<int, int> CurrentChunkTuple;
	public Vector3 Momentum; //Needs to be set when created or else will crash with NullReferenceException
	public bool PhysicsEnabled = true;
	public float Life = 0f;
	public Items.ID Type;

	public MeshInstance Mesh;

	public override void _Ready()
	{
		Mesh = GetNode<MeshInstance>("MeshInstance");

		ShaderMaterial Mat = new ShaderMaterial();
		Mat.Shader = Items.StructureShader;
		Mat.SetShaderParam("texture_albedo", Items.Textures[Type]);
		GetNode<MeshInstance>("MeshInstance").MaterialOverride = Mat;

		CurrentChunkTuple = World.GetChunkTuple(Translation);
	}


	public void GridUpdate()
	{
		PhysicsEnabled = true;
		World.Grid.QueueRemoveItem(this);
	}


	public void ApplyPush(Vector3 Push)
	{
		Momentum += Push;
		GridUpdate();

		if(Net.Work.IsNetworkServer())
			Net.SteelRpc(World.Self, nameof(World.DropOrUpdateItem), Type, Translation, Momentum, GetName());
	}


	public void Remove()
	{
		World.Self.RemoveDroppedItem(this.GetName());
		World.ItemList.Remove(this);
	}


	public override void _PhysicsProcess(float Delta)
	{
		Mesh.SetRotationDegrees(new Vector3(0, Mesh.RotationDegrees.y+(360*Delta*RPS), 0));
		Life += Delta;

		if(PhysicsEnabled)
		{
			Momentum = MoveAndSlide(Momentum, new Vector3(0,1,0), floorMaxAngle:50);
			if(!CurrentChunkTuple.Equals(World.GetChunkTuple(Translation))) //We just crossed into a different chunk
			{
				World.Chunks[CurrentChunkTuple].Items.Remove(this);
				CurrentChunkTuple = World.GetChunkTuple(Translation);
				World.AddItemToChunk(this);
				Net.SteelRpc(World.Self, nameof(World.DropOrUpdateItem), Type, Translation, Momentum, GetName());
			}

			if(IsOnFloor())
			{
				//Doing friction
				Vector3 Horz = new Vector3(Momentum.x, 0, Momentum.z);
				Horz = Horz.Normalized() * Mathf.Clamp(Horz.Length() - (Friction*Delta), 0, Mathf.Abs(MaxFallSpeed));
				Momentum.x = Horz.x;
				Momentum.z = Horz.z;

				if(Horz.Length() <= 0)
					PhysicsEnabled = false;
			}

			if(PhysicsEnabled)
			{
				Momentum.y -= Gravity*Delta;
				if(Momentum.y < MaxFallSpeed)
				{
					Momentum.y = MaxFallSpeed;
				}
			}
			else
			{
				Momentum = new Vector3(0,0,0);
				World.Grid.AddItem(this);
			}
		}
	}
}
