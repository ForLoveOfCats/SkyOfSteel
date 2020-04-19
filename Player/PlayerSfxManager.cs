using Godot;


public class PlayerSfxManager : Spatial
{
	public AudioStreamPlayer FpLandSfx;
	public AudioStreamPlayer3D TpLandSfx;

	public AudioStreamPlayer FpThrowSfx;
	public AudioStreamPlayer3D TpThrowSfx;

	public AudioStreamPlayer FpRocketFireSfx;
	public AudioStreamPlayer3D TpRocketFireSfx;

	public AudioStreamPlayer FpHitsoundSfx;
	public AudioStreamPlayer FpKillsoundSfx;

	public AudioStreamPlayer FpThunderboltFireSfx;
	public AudioStreamPlayer3D TpThunderboltFireSfx;

	public AudioStreamPlayer FpScattershockFireSfx;
	public AudioStreamPlayer3D TpScattershockFireSfx;

	public override void _Ready()
	{
		FpLandSfx = GetNode<AudioStreamPlayer>("FpLandSfx");
		TpLandSfx = GetNode<AudioStreamPlayer3D>("TpLandSfx");

		FpThrowSfx = GetNode<AudioStreamPlayer>("FpThrowSfx");
		TpThrowSfx = GetNode<AudioStreamPlayer3D>("TpThrowSfx");

		FpRocketFireSfx = GetNode<AudioStreamPlayer>("FpRocketFireSfx");
		TpRocketFireSfx = GetNode<AudioStreamPlayer3D>("TpRocketFireSfx");

		FpHitsoundSfx = GetNode<AudioStreamPlayer>("FpHitsoundSfx");
		FpKillsoundSfx = GetNode<AudioStreamPlayer>("FpKillsoundSfx");

		FpThunderboltFireSfx = GetNode<AudioStreamPlayer>("FpThunderboltFireSfx");
		TpThunderboltFireSfx = GetNode<AudioStreamPlayer3D>("TpThunderboltFireSfx");

		FpScattershockFireSfx = GetNode<AudioStreamPlayer>("FpScattershockFireSfx");
		TpScattershockFireSfx = GetNode<AudioStreamPlayer3D>("TpScattershockFireSfx");
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
	public void TpThrow()
	{
		TpThrowSfx.Play();
	}


	public void FpThrow()
	{
		FpThrowSfx.Play();
		Net.SteelRpc(this, nameof(TpThrow));
	}


	[Remote]
	public void TpRocketFire()
	{
		TpRocketFireSfx.Play();
	}


	public void FpRocketFire()
	{
		FpRocketFireSfx.Play();
		Net.SteelRpc(this, nameof(TpRocketFire));
	}


	public void FpHitsound()
	{
		FpHitsoundSfx.Play();
	}


	public void FpKillsound()
	{
		FpKillsoundSfx.Play();
	}


	[Remote]
	public void TpThunderboltFire()
	{
		TpThunderboltFireSfx.Play();
	}


	public void FpThunderboltFire()
	{
		FpThunderboltFireSfx.Play();
		Net.SteelRpc(this, nameof(TpThunderboltFire));
	}


	[Remote]
	public void TpScattershockFire()
	{
		TpScattershockFireSfx.Play();
	}


	public void FpScattershockFire()
	{
		FpScattershockFireSfx.Play();
		Net.SteelRpc(this, nameof(TpScattershockFire));
	}
}
