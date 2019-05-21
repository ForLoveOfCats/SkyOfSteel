using Godot;


public class SlotButton : Button
{
	public HostMenu HostMenuInstance; //Set externally by HostMenu

	public AudioStreamPlayer MouseOverSfx;

	public override void _Ready()
	{
		MouseOverSfx = GetNode<AudioStreamPlayer>("MouseOverSfx");
	}


	public void ButtonPressed()
	{
		HostMenuInstance.SelectSave(Text);
	}


	public void MouseEnter()
	{
		MouseOverSfx.Play();
	}
}
