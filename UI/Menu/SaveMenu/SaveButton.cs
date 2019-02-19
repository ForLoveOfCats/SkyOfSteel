using Godot;


public class SaveButton : Button
{
	public string SaveName = "";


	public void SavePressed()
	{
		Game.SaveWorld(SaveName);
		Menu.BuildPause();
	}
}
