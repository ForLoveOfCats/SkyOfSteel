using Godot;


public class PlayerSfxManager : Node
{
	public AudioStreamPlayer LandSfx;

	public override void _Ready()
	{
		LandSfx = GetNode<AudioStreamPlayer>("LandSfx");
	}


	public void FpLand(float Volume) //First person land sfx
	{
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(LandSfx.Bus), Volume);
		LandSfx.Play();
	}
}
