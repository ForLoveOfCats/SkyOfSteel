using Godot;
using System;
using System.Collections.Generic;



public class Mobs : Node {
	public enum ID { Slime }


	private static Dictionary<ID, PackedScene> Scenes = null;

	public static Mobs Self = null;

	private Mobs() {
		if(Engine.EditorHint) { return; }

		Self = this;

		Scenes = new Dictionary<ID, PackedScene> {
			{ID.Slime, GD.Load<PackedScene>("res://Mobs/Slime/SlimeMob.tscn")},
		};
	}


	public static void SpawnMob(ID Id, Vector3 Position) {
		if(Net.Work.IsNetworkServer())
			Self.RequestServerSpawnMob(Id, Position);
		else
			Self.RpcId(Net.ServerId, nameof(RequestServerSpawnMob), Id, Position);
	}


	[Remote]
	private void RequestServerSpawnMob(ID Id, Vector3 Position) {
		if(!Net.Work.IsNetworkServer())
			throw new Exception($"Attempted to run {nameof(RequestServerSpawnMob)} on client");

		//Do some server side housekeeping
		string GuidName = System.Guid.NewGuid().ToString();
		NetSpawnMob(Id, Position, GuidName);
		Net.SteelRpc(Self, nameof(NetSpawnMob), Id, Position, GuidName);
	}


	[Remote]
	public void NetSpawnMob(ID Id, Vector3 Position, string GuidName) {
		if(World.EntitiesRoot.HasNode(GuidName))
			return;

		var Mob = (MobClass)Scenes[Id].Instance();
		Mob.Type = Id;
		Mob.Translation = Position;
		Mob.Name = GuidName;
		World.AddMobToChunk(Mob);
		World.EntitiesRoot.AddChild(Mob);
	}
}
