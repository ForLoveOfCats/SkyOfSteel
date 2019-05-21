using Godot;


public class CreditsMenu : VBoxContainer
{
	public void LicensesPressed()
	{
		Menu.BuildLicenses();
	}


	public void BackPressed()
	{
		Menu.BuildMain();
	}
}
