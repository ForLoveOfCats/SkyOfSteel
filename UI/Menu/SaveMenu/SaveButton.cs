using Godot;


public class SaveButton : Button
{
	public string SaveName = "";


	public void SavePressed()
	{
		World.Save(SaveName);
		Menu.BuildPause();
	}
}
