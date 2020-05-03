using Godot;


public class PauseMenu : VBoxContainer {
	public Button TeamButton;
	public LineEdit TeamEdit;


	public override void _Ready() {
		TeamButton = GetNode<Button>("TeamSwitchBox/ChangeButton");
		TeamEdit = GetNode<LineEdit>("TeamSwitchBox/LineEdit");
		TeamEdit.Text = $"{Net.Players[Net.Work.GetNetworkUniqueId()].Team}";

		GetNode<Label>("Version").Text = $"Version: {Game.Version}";

		Label PlayingOn = GetNode<Label>("PlayingOn");
		if(Net.Work.IsNetworkServer()) {
			PlayingOn.Text = $"Hosting map: {World.SaveName}";
		}
		else {
			PlayingOn.Text = $"Connected to server at: {Net.Ip}";
		}

		if(Net.Work.IsNetworkServer()) {
			Button DisconnectButton = GetNode<Button>("DisconnectButton");
			DisconnectButton.Text = "Save and Disconnect";

			Button QuitButton = GetNode<Button>("QuitButton");
			QuitButton.Text = "Save and Quit";
		}
		else {
			Button SaveButton = GetNode<Button>("SaveButton");
			SaveButton.Disabled = true;
			SaveButton.HintTooltip = "Cannot save as client";
			SaveButton.MouseDefaultCursorShape = CursorShape.Arrow;
		}
	}


	public void ReturnPressed() {
		Menu.Close();
	}


	public void TeamChanged() {
		if(!int.TryParse(TeamEdit.Text, out int ProspectiveTeam)) {
			Console.ThrowLog("Attempted to change to a non-int team");
			return;
		}

		Net.Self.RequestTeamChange(ProspectiveTeam);
	}


	public void TeamChanged(string Text) {
		TeamChanged();
	}


	public void SavePressed() {
		if(Net.Work.IsNetworkServer())
			World.Save(World.SaveName);
	}


	public void DisconnectPressed() {
		if(Net.Work.IsNetworkServer() && World.SaveName != null)
			World.Save(World.SaveName);

		Net.Disconnect();
	}


	public void QuitPressed() {
		if(Net.Work.IsNetworkServer() && World.SaveName != null)
			World.Save(World.SaveName);

		Game.Quit();
	}


	public override void _Process(float Delta) {
		int ProspectiveTeam = 1;
		if(int.TryParse(TeamEdit.Text, out ProspectiveTeam))
			TeamButton.Disabled = false;
		else
			TeamButton.Disabled = true;
	}
}
