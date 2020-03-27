using Godot;
using System;
using System.Collections.Generic;



public class Mobs : Node
{
	public enum ID {Slime}


	private static Dictionary<ID, PackedScene> Scenes = null;

	public static Mobs Self = null;

	private Mobs()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		Scenes = new Dictionary<ID, PackedScene> {
			{ID.Slime, GD.Load<PackedScene>("res://Mobs/Slime/SlimeMob.tscn")},
		};
	}


	public static void SpawnMob(ID Id)
	{
		if(Net.Work.IsNetworkServer())
			Self.RequestServerSpawnMob(Id);
		else
			Self.RpcId(Net.ServerId, nameof(RequestServerSpawnMob), Id);
	}


	[Remote]
	private void RequestServerSpawnMob(ID Id)
	{
		if(!Net.Work.IsNetworkServer())
			throw new Exception($"Attempted to run {nameof(RequestServerSpawnMob)} on client");

		//Do some server side housekeeping
		string GuidName = System.Guid.NewGuid().ToString();
		NetSpawnMob(Id, GuidName);
		Net.SteelRpc(Self, nameof(NetSpawnMob), Id, GuidName);
	}


	[Remote]
	public void NetSpawnMob(ID Id, string GuidName)
	{
		if(World.EntitiesRoot.HasNode(GuidName))
			return;

		var Mob = (MobClass) Scenes[Id].Instance();
		Mob.Type = Id;
		Mob.Translation = new Vector3(0, 2, 0);
		Mob.Name = GuidName;
		World.AddMobToChunk(Mob);
		World.EntitiesRoot.AddChild(Mob);
	}
}
