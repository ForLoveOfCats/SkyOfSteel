using Godot;
using System.Collections.Generic;


public class HUD : Node
{
	private Texture Alpha;
	private Texture Triangle;
	private PackedScene NickLabelScene;

	private Dictionary<int, Label> NickLabels = new Dictionary<int, Label>();

	private TextureRect Crosshair;
	private Label ChunkInfoLabel;
	private Label PlayerPositionLabel;
	private Label FPSLabel;

	HUD()
	{
		if(Engine.EditorHint) {return;}

		Alpha = GD.Load("res://UI/Textures/Alpha.png") as Texture;
		Triangle = GD.Load("res://UI/Textures/Triangle.png") as Texture;
		NickLabelScene = GD.Load<PackedScene>("res://UI/NickLabel.tscn");
	}


	public override void _Ready()
	{
		Crosshair = GetNode<TextureRect>("CLayer/CrossCenter/TextureRect");
		ChunkInfoLabel = GetNode<Label>("CLayer/ChunkInfo");
		PlayerPositionLabel = GetNode<Label>("CLayer/PlayerPosition");
		FPSLabel = GetNode<Label>("CLayer/FPSLabel");

		GetNode<Label>("CLayer/VersionLabel").Text = $"Version: {Game.Version}";

		GetTree().Connect("screen_resized", this, nameof(OnScreenResized));
		HotbarUpdate();
		this.CallDeferred(nameof(OnScreenResized));
	}


	public void HotbarUpdate()
	{
		for(int Slot = 0; Slot <= 9; Slot++)
		{
			NinePatchRect SlotPatch = GetNode("CLayer/HotBarCenter/HBoxContainer/Vbox").GetChild(Slot) as NinePatchRect;
			if(Game.PossessedPlayer.Inventory[Slot] != null)
			{
				SlotPatch.Texture = Items.Thumbnails[Game.PossessedPlayer.Inventory[Slot].Type];
			}
			else
			{
				SlotPatch.Texture = Alpha;
			}

			SlotPatch.RectMinSize = new Vector2(GetViewport().Size.y/11, GetViewport().Size.y/11);

			NinePatchRect ActiveIndicatorPatch = GetNode("CLayer/HotBarCenter/HBoxContainer/Vbox2").GetChild(Slot) as NinePatchRect;
			ActiveIndicatorPatch.RectMinSize = new Vector2(GetViewport().Size.y/11, GetViewport().Size.y/11);
			ActiveIndicatorPatch.Texture = Alpha;
		}

		((NinePatchRect)(GetNode("CLayer/HotBarCenter/HBoxContainer/Vbox2").GetChild(Game.PossessedPlayer.InventorySlot))).Texture = Triangle;
	}


	public void OnScreenResized() //Not an override
	{
		HotbarUpdate();
	}


	private void HideNodes(Godot.Collections.Array Nodes)
	{
		foreach(Node ToHide in Nodes)
		{
			if(ToHide is CanvasItem)
			{
				((CanvasItem)ToHide).Hide();
			}
			HideNodes(ToHide.GetChildren());
		}
	}


	public void Hide()
	{
		HideNodes(GetChildren());
	}


	private void ShowNodes(Godot.Collections.Array Nodes)
	{
		foreach(Node ToShow in Nodes)
		{
			if(ToShow is CanvasItem)
			{
				((CanvasItem)ToShow).Show();
			}
			ShowNodes(ToShow.GetChildren());
		}
	}


	public void Show()
	{
		ShowNodes(GetChildren());
	}


	public void AddNickLabel(int Id, string Nick)
	{
		Label Instance = NickLabelScene.Instance() as Label;
		Instance.Text = Nick;
		AddChild(Instance);
		NickLabels[Id] = Instance;
	}


	public void RemoveNickLabel(int Id)
	{
		if(NickLabels.ContainsKey(Id))
		{
			NickLabels[Id].QueueFree();
			NickLabels.Remove(Id);
		}
	}


	public override void _Process(float Delta)
	{
		Crosshair.Visible = !Menu.IsOpen;

		foreach(KeyValuePair<int, Label> Current in NickLabels)
		{
			Vector3 PlayerPos = Net.Players[Current.Key].Translation + new Vector3(0,7.5f,0);
			if(Game.PossessedPlayer.Cam.IsPositionBehind(PlayerPos))
			{
				Current.Value.Visible = false;
			}
			else
			{
				Current.Value.Visible = true;
				Current.Value.MarginLeft = Game.PossessedPlayer.Cam.UnprojectPosition(PlayerPos).x - Current.Value.RectSize.x/2;
				Current.Value.MarginTop = Game.PossessedPlayer.Cam.UnprojectPosition(PlayerPos).y - Current.Value.RectSize.y/2;
			}
		}

		ChunkInfoLabel.Text = $"Current Chunk: ({Game.PossessedPlayer.CurrentChunk.Item1}, 0, {Game.PossessedPlayer.CurrentChunk.Item2})";
		PlayerPositionLabel.Text = $"Player Position: {Game.PossessedPlayer.Translation.Round()}";
		FPSLabel.Text = $"{Engine.GetFramesPerSecond()} fps";
	}
}
