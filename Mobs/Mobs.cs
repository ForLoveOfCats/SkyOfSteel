using Godot;
using System;
using System.Collections.Generic;



public class Mobs : Node
{
	public enum ID {Cat}


	private static Dictionary<ID, PackedScene> Scenes = null;

	public static Mobs Self = null;

	private Mobs()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		Scenes = new Dictionary<ID, PackedScene> {
			{ID.Cat, GD.Load<PackedScene>("res://Mobs/CatMob.tscn")},
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
		Net.SteelRpc(Self, nameof(RequestServerSpawnMob), Id, Name);
	}


	private void NetSpawnMob(ID Id, string Name)
	{
		Mob Instance = Scenes[Id].Instance() as Mob;
		Instance.Name = Name;
		World.MobsRoot.AddChild(Instance);
	}
}
