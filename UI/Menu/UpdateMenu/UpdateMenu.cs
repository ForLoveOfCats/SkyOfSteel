using Godot;
using System;


public class UpdateMenu : VBoxContainer {
	public override void _Ready() {
		Label MessageLabel = GetNode<Label>("MessageLabel");
		MessageLabel.Text = $"You have {Game.Version} and {Game.RemoteVersion} is available";
	}


	public void DownloadPressed() {
		OS.ShellOpen("https://forloveofcats.itch.io/skyofsteel");
	}


	public void IgnorePressed() {
		if(Game.Nickname != Game.DefaultNickname)
			Menu.BuildMain();
		else
			Menu.BuildNick();
	}


	public void QuitPressed() {
		Game.Quit();
	}
}
