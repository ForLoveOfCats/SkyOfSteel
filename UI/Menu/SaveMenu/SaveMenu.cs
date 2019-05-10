using Godot;
using System.Collections.Generic;

public class SaveMenu : VBoxContainer
{
	PackedScene SaveButtonScene;
	PackedScene LabelPieceScene;

	public override void _Ready()
	{
		SaveButtonScene = GD.Load<PackedScene>("res://UI/Menu/SaveMenu/SaveButton.tscn");
		LabelPieceScene = GD.Load<PackedScene>("res://UI/Menu/Pieces/LabelPiece.tscn");

		Directory SaveDir = new Directory();
		if(SaveDir.DirExists("user://Saves"))
		{
			List<string> Names = new List<string>();
			SaveDir.Open("user://Saves");
			SaveDir.ListDirBegin(skipNavigational: true, skipHidden: true);
			while(true)
			{
				string SaveName = SaveDir.GetNext();
				if(SaveName == "")
				{
					break;
				}
				Names.Add(SaveName);
			}

			foreach(string Name in Names)
			{
				SaveButton Instanced = SaveButtonScene.Instance() as SaveButton;
				Instanced.Text = Name;
				Instanced.SaveName = Name;
				AddChildBelowNode(GetNode("ButtonsBelow"), Instanced);
			}
		}
		else
		{
			Label Message = LabelPieceScene.Instance() as Label;
			Message.Text = "No saves to overwrite";
			AddChildBelowNode(GetNode("ButtonsBelow"), Message);
		}
	}


	public void SavePressed()
	{
		LineEdit NameInput = GetNode<LineEdit>("HBoxContainer/LineEdit");
		World.Save(NameInput.Text);

		Menu.BuildPause();
	}


	public void BackPressed()
	{
		Menu.BuildPause();
	}
}
