using Godot;


public class ButtonPiece : Button
{
	public AudioStreamPlayer MouseOverSfx;

	public override void _Ready()
	{
		MouseOverSfx = GetNode<AudioStreamPlayer>("MouseOverSfx");
	}


	public void MouseEnter()
	{
		if(!Disabled)
			MouseOverSfx.Play();
	}
}
