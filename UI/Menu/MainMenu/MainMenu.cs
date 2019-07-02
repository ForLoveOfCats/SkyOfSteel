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


	public void WebsitePressed()
	{
		OS.ShellOpen("https://skyofsteel.org");
	}


	public void DiscordPressed()
	{
		OS.ShellOpen("https://www.discord.gg/Ag5Yckw");
	}


	public void ItchPressed()
	{
		OS.ShellOpen("https://forloveofcats.itch.io/skyofsteel");
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
