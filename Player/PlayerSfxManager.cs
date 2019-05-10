using Godot;


public class PlayerSfxManager : Spatial
{
	public AudioStreamPlayer FpLandSfx;
	public AudioStreamPlayer3D TpLandSfx;

	public AudioStreamPlayer FpWallKickSfx;
	public AudioStreamPlayer3D TpWallKickSfx;

	public override void _Ready()
	{
		FpLandSfx = GetNode<AudioStreamPlayer>("FpLandSfx");
		TpLandSfx = GetNode<AudioStreamPlayer3D>("TpLandSfx");

		FpWallKickSfx = GetNode<AudioStreamPlayer>("FpWallKickSfx");
		TpWallKickSfx = GetNode<AudioStreamPlayer3D>("TpWallKickSfx");
	}


	[Remote]
	public void TpLand(float Volume)
	{
		TpLandSfx.UnitDb = Volume + 10;
		TpLandSfx.Play();
	}


	public void FpLand(float Volume) //First person land sfx
	{
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex(FpLandSfx.Bus), Volume);
		FpLandSfx.Play();
		Net.SteelRpc(this, nameof(TpLand), Volume);
	}


	[Remote]
	public void TpWallKick()
	{
		TpWallKickSfx.Play();
	}


	public void FpWallKick()
	{
		FpWallKickSfx.Play();
		Net.SteelRpc(this, nameof(TpWallKick));
	}
}
