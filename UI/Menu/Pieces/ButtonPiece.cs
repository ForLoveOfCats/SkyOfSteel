using Godot;


public class ButtonPiece : Button
{
	public void MouseEnter()
	{
		if(!Disabled)
			Sfx.Play(Sfx.ID.BUTTON_MOUSEOVER);
	}
}
