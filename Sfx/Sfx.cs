using Godot;
using System;
using System.Collections.Generic;



public class Sfx : Node
{
	private class OmnispresentPlayer : AudioStreamPlayer
	{
		public OmnispresentPlayer(Sfx.ID ClipId) : base()
		{
			Stream = Clips[ClipId];
			Play();
			Connect("finished", this, nameof(OnFinished));
		}


		private void OnFinished()
		{
			QueueFree();
		}
	}



	public enum ID
	{
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

	private Sfx()
	{
		if(Engine.EditorHint) {return;}

		Self = this;

		foreach(ID ClipId in Enum.GetValues(typeof(ID)))
		{
			var Sample = GD.Load<AudioStreamSample>($"res://Sfx/Clips/{ClipId}.wav");
			if(Sample is null)
				throw new Exception($"Sfx for {ClipId} not found");
			Clips.Add(ClipId, Sample);
		}
	}


	//Play a clip omnipresently
	//NOTE: Has no networked counterpart yet
	public static void Play(ID ClipId)
	{
		Self.AddChild(new OmnispresentPlayer(ClipId));
	}
}
