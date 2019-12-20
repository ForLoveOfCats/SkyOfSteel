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

		//Do some serverside housekeeping
		string Name = System.Guid.NewGuid().ToString();
		NetSpawnMob(Id, Name);
		Net.SteelRpc(Self, nameof(NetSpawnMob), Id, Name);
	}


	[Remote]
	public void NetSpawnMob(ID Id, string Name)
	{
		MobClass Mob = Scenes[Id].Instance() as MobClass;
		Mob.Type = Id;
		Mob.Translation = new Vector3(0, 2, 0);
		Mob.Name = Name;
		World.AddMobToChunk(Mob);
		World.MobsRoot.AddChild(Mob);
	}
}
