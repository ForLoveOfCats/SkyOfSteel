using Godot;
using System;
using System.Collections.Generic;



public class Sfx : Node {
	private class AudioPlayerOmnispresent : AudioStreamPlayer {
		public AudioPlayerOmnispresent(Sfx.ID ClipId) : base() {
			Stream = Clips[ClipId];
			Play();
			Connect("finished", this, nameof(OnFinished));
		}


		private void OnFinished() {
			QueueFree();
		}
	}



	private class AudioPlayer3D : AudioStreamPlayer3D {
		public AudioPlayer3D(Sfx.ID ClipId, Vector3 Position) : base() {
			Translation = Position;
			Stream = Clips[ClipId];
			Play();
			Connect("finished", this, nameof(OnFinished));
		}


		private void OnFinished() {
			QueueFree();
		}
	}



	public enum ID {
		BUTTON_MOUSEOVER,
		HITSOUND,
		KILLSOUND,
		THUNDERBOLT_FIRE,
		SCATTERSHOCK_FIRE,
		ROCKET_JUMPER_FIRE,
		ROCKET_JUMPER_EXPLODE,
		ITEM_THROW,
		ITEM_PICKUP,
		PLAYER_LAND,
		PLAYER_WALLKICK, //Old and unused, TODO: use the clip for something else
	}

	public static Dictionary<ID, AudioStreamSample> Clips { get; private set; } = new Dictionary<ID, AudioStreamSample>();


	public static Sfx Self;

	private Sfx() {
		if(Engine.EditorHint) { return; }

		Self = this;

		foreach(ID ClipId in Enum.GetValues(typeof(ID))) {
			var Sample = GD.Load<AudioStreamSample>($"res://Sfx/Clips/{ClipId}.wav");
			if(Sample is null)
				throw new Exception($"Sfx for {ClipId} not found");
			Clips.Add(ClipId, Sample);
		}
	}


	//Play a clip omnipresently
	//NOTE: Has no networked counterpart
	public static void Play(ID ClipId) {
		Self.AddChild(new AudioPlayerOmnispresent(ClipId));
	}


	public static void PlayPlayer(ID ClipId, Player Plr) {
		Play(ClipId);
		PlayAt(ClipId, Plr.Translation);
	}


	public static void PlayAt(ID ClipId, Vector3 Position) {
		Self.ActualPlayAt(ClipId, Position);

		if(Net.Work.IsNetworkServer())
			Self.PleasePlayAtForOthers(ClipId, Position);
		else
			Self.RpcId(Net.ServerId, nameof(PleasePlayAtForOthers), ClipId, Position);
	}


	[Remote]
	private void PleasePlayAtForOthers(ID ClipId, Vector3 Position) {
		if(!Net.Work.IsNetworkServer()) {
			Console.ThrowLog($"Attempted to execute {nameof(PleasePlayAtForOthers)} on client");
			return;
		}

		var AudioChunk = World.GetChunkTuple(Position);

		if(Net.Work.GetRpcSenderId() != 0) //Is an RPC, we did not trigger it ourself
		{
			//Play on ourself if the audio's chunk is within render distance
			Game.PossessedPlayer.MatchSome(
				(Plr) => {
					if(World.ChunkWithinDistanceFrom(AudioChunk, Game.ChunkRenderDistance, Plr.Translation))
						ActualPlayAt(ClipId, Position);
				}
			);
		}

		//Play on all clients which have the audio's chunk within render distance
		foreach(KeyValuePair<int, Net.PlayerData> KV in Net.Players) {
			int Receiver = KV.Key;
			if(Receiver == Net.Work.GetNetworkUniqueId() || Receiver == Net.Work.GetRpcSenderId())
				continue;

			KV.Value.Plr.MatchSome(
				(Plr) => {
					int ChunkRenderDistance = World.ChunkRenderDistances[Receiver];
					if(World.ChunkWithinDistanceFrom(AudioChunk, ChunkRenderDistance, Plr.Translation))
						Self.RpcId(Receiver, nameof(ActualPlayAt), ClipId, Position);
				}
			);
		}
	}


	[Remote]
	private void ActualPlayAt(ID ClipId, Vector3 Position) {
		Self.AddChild(new AudioPlayer3D(ClipId, Position));
	}
}
