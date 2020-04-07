using Godot;
using static Godot.Mathf;



public class DroppedItem : KinematicBody, IEntity, IInGrid, IPushable
{
	private const float Gravity = 50f;
	private const float MaxFallSpeed = 100f;
	private const float Friction = 48f;
	private const float RPS = 0.5f; //Revolutions Per Second
	public static float MinPickupLife = 0.6f; //In seconds

	public System.Tuple<int, int> CurrentChunkTuple;
	public System.Tuple<int, int> CurrentChunk { get; set; } //TODO: This is for IEntity, unify
	public Vector3 Momentum; //Needs to be set when created or else will crash with NullReferenceException
	public bool PhysicsEnabled = true;
	public float Life = 0f;
	public Items.ID Type;

	public MeshInstance Mesh;

	public override void _Ready()
	{
		Mesh = GetNode<MeshInstance>("MeshInstance");

		ShaderMaterial Mat = new ShaderMaterial();
		Mat.Shader = Items.TileShader;
		Mat.SetShaderParam("texture_albedo", Items.Textures[Type]);
		GetNode<MeshInstance>("MeshInstance").MaterialOverride = Mat;

		CurrentChunkTuple = World.GetChunkTuple(Translation);
		World.AddEntityToChunk(this);
	}


	public override void _ExitTree()
	{
		World.RemoveEntityFromChunk(this);
	}


	[Remote]
	public void PhaseOut()
	{
		World.RemoveDroppedItem(Name);
	}


	[Remote]
	public void Destroy(params object[] Args)
	{
		Assert.ArgArray(Args);
		World.RemoveDroppedItem(Name);
	}


	[Remote]
	public void Update(params object[] Args)
	{
		Assert.ArgArray(Args, typeof(Vector3));
		var OriginalChunkTuple = World.GetChunkTuple(Translation);
		Translation = (Vector3)Args[0];
		Entities.MovedTick(this, OriginalChunkTuple);
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
			Net.SteelRpc(World.Self, nameof(World.DropOrUpdateItem), Type, Translation, RotationDegrees.y, Momentum, Name);
	}


	public void Remove()
	{
		World.RemoveDroppedItem(Name);
		World.ItemList.Remove(this);
	}


	public override void _PhysicsProcess(float Delta)
	{
		Mesh.RotationDegrees = new Vector3(0, Mesh.RotationDegrees.y+(360*Delta*RPS), 0);
		Life += Delta;

		if(!Net.Work.IsNetworkServer())
			return;

		if(PhysicsEnabled)
		{
			var OriginalChunkTuple = World.GetChunkTuple(Translation);

			Momentum = MoveAndSlide(Momentum, new Vector3(0,1,0), floorMaxAngle:Deg2Rad(60));
			if(!CurrentChunkTuple.Equals(World.GetChunkTuple(Translation))) //We just crossed into a different chunk
			{
				World.Chunks[CurrentChunkTuple].Items.Remove(this);
				CurrentChunkTuple = World.GetChunkTuple(Translation);
				World.AddItemToChunk(this);
				Net.SteelRpc(World.Self, nameof(World.DropOrUpdateItem), Type, Translation, RotationDegrees.y, Momentum, Name);
			}

			if(IsOnFloor())
			{
				//Doing friction
				Vector3 Horz = new Vector3(Momentum.x, 0, Momentum.z);
				Horz = Horz.Normalized() * Mathf.Clamp(Horz.Length() - (Friction*Delta), 0, MaxFallSpeed);
				Momentum.x = Horz.x;
				Momentum.z = Horz.z;

				if(Horz.Length() <= 0)
					PhysicsEnabled = false;
			}

			if(PhysicsEnabled)
			{
				Momentum.y -= Gravity*Delta;
				if(Momentum.y < -MaxFallSpeed)
				{
					Momentum.y = -MaxFallSpeed;
				}
			}
			else
			{
				Momentum = new Vector3(0,0,0);
				World.Grid.AddItem(this);
			}

			Entities.MovedTick(this, OriginalChunkTuple);
		}

		Entities.AsServerMaybePhaseOut(this);
		Entities.SendUpdate(Name, Translation);
	}
}
