using Godot;


public class SlotButton : Button {
	public HostMenu HostMenuInstance; //Set externally by HostMenu

	public void ButtonPressed() {
		HostMenuInstance.SelectSave(Text);
	}


	public void MouseEnter() {
		Sfx.Play(Sfx.ID.BUTTON_MOUSEOVER);
	}
}
