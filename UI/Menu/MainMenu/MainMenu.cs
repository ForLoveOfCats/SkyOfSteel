using Godot;
using System;

public class MainMenu : VBoxContainer
{
	public void DiscordPressed()
	{
		OS.ShellOpen("https://www.discord.gg/Ag5Yckw");
	}

	public void TwitterPressed()
	{
		OS.ShellOpen("https://twitter.com/ForLoveOfCats");
	}


	public void GithubPressed()
	{
		OS.ShellOpen("https://github.com/ForLoveOfCats/SkyOfSteel");
	}


	public void QuitPressed()
	{
		Game.Quit();
	}
}
