using Godot;


public class PlayerSfxManager : Spatial
{
	public AudioStreamPlayer FpLandSfx;
	public AudioStreamPlayer3D TpLandSfx;

	public AudioStreamPlayer FpWallKickSfx;
	public AudioStreamPlayer3D TpWallKickSfx;

	public AudioStreamPlayer FpThrowSfx;
	public AudioStreamPlayer3D TpThrowSfx;

	public AudioStreamPlayer FpPickupSfx;
	public AudioStreamPlayer3D TpPickupSfx;

	public AudioStreamPlayer FpRocketFireSfx;
	public AudioStreamPlayer3D TpRocketFireSfx;

	public override void _Ready()
	{
		FpLandSfx = GetNode<AudioStreamPlayer>("FpLandSfx");
		TpLandSfx = GetNode<AudioStreamPlayer3D>("TpLandSfx");

		FpWallKickSfx = GetNode<AudioStreamPlayer>("FpWallKickSfx");
		TpWallKickSfx = GetNode<AudioStreamPlayer3D>("TpWallKickSfx");

		FpThrowSfx = GetNode<AudioStreamPlayer>("FpThrowSfx");
		TpThrowSfx = GetNode<AudioStreamPlayer3D>("TpThrowSfx");

		FpPickupSfx = GetNode<AudioStreamPlayer>("FpPickupSfx");
		TpPickupSfx = GetNode<AudioStreamPlayer3D>("TpPickupSfx");

		FpRocketFireSfx = GetNode<AudioStreamPlayer>("FpRocketFireSfx");
		TpRocketFireSfx = GetNode<AudioStreamPlayer3D>("TpRocketFireSfx");
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
	public void TpPickup()
	{
		TpPickupSfx.Play();
	}


	public void FpPickup()
	{
		FpPickupSfx.Play();
		Net.SteelRpc(this, nameof(TpPickup));
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
}
