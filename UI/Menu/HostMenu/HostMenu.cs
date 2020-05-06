using Godot;
using System.Collections.Generic;

public class HostMenu : VBoxContainer {
	PackedScene SlotButtonScene;
	PackedScene LabelPieceScene;

	public Label SelectedSaveLabel;
	public HBoxContainer RenameToolbar;
	public LineEdit RenameEdit;
	public HBoxContainer CreateToolbar;
	public LineEdit CreateEdit;
	public HBoxContainer DeleteToolbar;
	public Label DeleteMessage;
	public Label ToolbarScrollSeperator;
	public VBoxContainer SlotsVBox;

	public string SelectedSave = null;

	public override void _Ready() {
		SlotButtonScene = GD.Load<PackedScene>("res://UI/Menu/HostMenu/SlotButton.tscn");
		LabelPieceScene = GD.Load<PackedScene>("res://UI/Menu/Pieces/LabelPiece.tscn");

		SelectedSaveLabel = GetNode<Label>("SelectedSave");
		ResetSelectedSave();

		RenameToolbar = GetNode<HBoxContainer>("RenameToolbar");
		RenameEdit = RenameToolbar.GetNode<LineEdit>("NameLineEdit");
		RenameToolbar.Hide();

		CreateToolbar = GetNode<HBoxContainer>("CreateToolbar");
		CreateEdit = CreateToolbar.GetNode<LineEdit>("NameLineEdit");
		CreateToolbar.Hide();

		DeleteToolbar = GetNode<HBoxContainer>("DeleteToolbar");
		DeleteMessage = DeleteToolbar.GetNode<Label>("Label");
		DeleteToolbar.Hide();

		ToolbarScrollSeperator = GetNode<Label>("ToolbarScrollSeperator");
		ToolbarScrollSeperator.Hide();

		SlotsVBox = GetNode<VBoxContainer>("SlotsScroll/SlotsVBox");
		ResetSlotsVBox();
	}


	public void ResetSelectedSave() {
		SelectedSave = null;
		SelectedSaveLabel.Text = "No save selected";
	}


	public void ResetSlotsVBox() {
		foreach(Node Child in SlotsVBox.GetChildren()) {
			Child.QueueFree();
		}

		Directory SaveDir = new Directory();
		List<string> Names = new List<string>();
		if(SaveDir.DirExists("user://Saves")) {
			SaveDir.Open("user://Saves");
			SaveDir.ListDirBegin(skipNavigational: true, skipHidden: true);
			while(true) {
				string SaveName = SaveDir.GetNext();
				if(SaveName == "") {
					break;
				}
				Names.Add(SaveName);
			}
			Names.Sort();

			foreach(string Name in Names) {
				var Instanced = (SlotButton)SlotButtonScene.Instance();
				Instanced.HostMenuInstance = this;
				Instanced.Text = Name;
				SlotsVBox.AddChild(Instanced);
			}
		}

		if(Names.Count <= 0) {
			var Message = (Label)LabelPieceScene.Instance();
			Message.Text = "No saves to load";
			SlotsVBox.AddChild(Message);
		}
	}


	public void SelectSave(string NameArg) {
		SelectedSave = NameArg;
		SelectedSaveLabel.Text = $"Save currently selected: {SelectedSave}";
	}


	public void LoadPressed() {
		if(SelectedSave != null) {
			Net.Host(Dedicated: false);
			World.Load(SelectedSave);
		}
	}


	public void NewPressed() {
		CloseRenameToolbar();
		CloseDeleteToolbar();

		CreateToolbar.Show();
		CreateEdit.Clear();
		CreateEdit.GrabFocus();

		ToolbarScrollSeperator.Show();
	}


	public void ConfirmCreatePressed() {
		if(!string.IsNullOrEmpty(CreateEdit.Text) && !string.IsNullOrWhiteSpace(CreateEdit.Text)) {
			System.IO.Directory.CreateDirectory($"{OS.GetUserDataDir()}/Saves/{CreateEdit.Text}");

			CloseCreateToolbar();
			ResetSlotsVBox();
		}
	}


	public void CloseCreateToolbar() {
		CreateToolbar.Hide();
		ToolbarScrollSeperator.Hide();
	}


	public void RenamePressed() {
		if(SelectedSave != null) {
			CloseCreateToolbar();
			CloseDeleteToolbar();

			RenameToolbar.Show();
			RenameEdit.Clear();
			RenameEdit.GrabFocus();

			ToolbarScrollSeperator.Show();
		}
	}


	public void ConfirmRenamePressed() {
		if(!string.IsNullOrEmpty(RenameEdit.Text) && !string.IsNullOrWhiteSpace(RenameEdit.Text)) {
			string Source = $"{OS.GetUserDataDir()}/Saves/{SelectedSave}";
			string Destination = $"{OS.GetUserDataDir()}/Saves/{RenameEdit.Text}";
			if(System.IO.Directory.Exists(Source) && !System.IO.Directory.Exists(Destination)) {
				System.IO.Directory.Move(Source, Destination);
			}

			CloseRenameToolbar();
			ResetSelectedSave();
			ResetSlotsVBox();
		}
	}


	public void CloseRenameToolbar() {
		RenameToolbar.Hide();
		ToolbarScrollSeperator.Hide();
	}


	public void DeletePressed() {
		if(SelectedSave != null) {
			CloseCreateToolbar();
			CloseRenameToolbar();

			DeleteToolbar.Show();
			DeleteMessage.Text = $"Are you sure you want to delete save: {SelectedSave}";

			ToolbarScrollSeperator.Show();
		}
	}


	public void ConfirmDeletePressed() {
		if(SelectedSave != null) {
			System.IO.Directory.Delete($"{OS.GetUserDataDir()}/Saves/{SelectedSave}", true);
			ResetSelectedSave();
			ResetSlotsVBox();
		}

		CloseDeleteToolbar();
	}


	public void CloseDeleteToolbar() {
		DeleteToolbar.Hide();
		ToolbarScrollSeperator.Hide();
	}


	public void BackPressed() {
		Menu.BuildMain();
	}
}
