using Godot;


public class PlayerSfxManager : Spatial
{
	public AudioStreamPlayer FpLandSfx;
	public AudioStreamPlayer3D TpLandSfx;

	public override void _Ready()
	{
		FpLandSfx = GetNode<AudioStreamPlayer>("FpLandSfx");
		TpLandSfx = GetNode<AudioStreamPlayer3D>("TpLandSfx");
	}


	[Remote]
	public void TpLand(float Volume)
	{
		TpLandSfx.UnitDb = Volume + 10;
		TpLandSfx.Play();
	}


	public void FpLand(float Volume) //First person land sfx
	{
		GD.Print(Volume);
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(FpLandSfx.Bus), Volume);
		FpLandSfx.Play();
		Net.SteelRpc(this, nameof(TpLand), Volume);
	}
}
