using Godot;
using System.Collections.Generic;

public class SavedWorldHost : VBoxContainer
{
	PackedScene LoadButtonScene;
	PackedScene LabelPieceScene;

	public override void _Ready()
	{
		LoadButtonScene = GD.Load<PackedScene>("res://UI/Menu/SavedWorldHost/LoadSaveHostButton.tscn");
		LabelPieceScene = GD.Load<PackedScene>("res://UI/Menu/Pieces/LabelPiece.tscn");

		Directory SaveDir = new Directory();
		if(SaveDir.DirExists("user://saves"))
		{
			List<string> Names = new List<string>();
			SaveDir.Open("user://saves");
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
				LoadSaveHostButton Instanced = LoadButtonScene.Instance() as LoadSaveHostButton;
				Instanced.Text = Name;
				Instanced.SaveName = Name;
				AddChildBelowNode(GetNode("ButtonsBelow"), Instanced);
			}
		}
		else
		{
			Label Message = LabelPieceScene.Instance() as Label;
			Message.Text = "No saves to load";
			AddChildBelowNode(GetNode("ButtonsBelow"), Message);
		}
	}


	public void BackPressed()
	{
		Menu.BuildHost();
	}
}
