using Godot;


public class WaitConnectingMenu : VBoxContainer {
	private Label Message;

	public override void _Ready() {
		Message = GetNode<Label>("Message");
	}


	public void ConnectFailed(string Ip) {
		Message.Text = $"Failed to connect to '{Ip}'";
	}


	public void CancelPressed() {
		Net.Disconnect();
		Menu.BuildMain();
	}


	public override void _Process(float Delta) {
		if(Net.IsWaitingForServer) {
			Message.Text = $"Attempting to connect to '{Net.Ip}'....      {(int)Net.WaitingForServerTimer}";
		}
	}
}
