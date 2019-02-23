using Godot;


public class MainMenu : VBoxContainer
{
	public override void _Ready()
	{
		GetNode<Label>("Version").Text = $"Version: {Game.Version}";
	}


	public void HostPressed()
	{
		Menu.BuildHost();
	}


	public void ConnectPressed()
	{
		Menu.BuildConnect();
	}


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


	public void HelpPressed()
	{
		Menu.BuildHelp();
	}


	public void FilesPressed()
	{
		OS.ShellOpen(OS.GetUserDataDir());
	}


	public void CreditsPressed()
	{
		Menu.BuildCredits();
	}


	public void QuitPressed()
	{
		Game.Quit();
	}
}
