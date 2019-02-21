using Godot;
using System.Collections.Generic;


public class HUD : Node
{
	private Texture Alpha;
	private Texture Triangle;
	private PackedScene NickLabelScene;

	private Dictionary<int, Label> NickLabels = new Dictionary<int, Label>();

	private TextureRect Crosshair;

	HUD()
	{
		if(Engine.EditorHint) {return;}

		Alpha = GD.Load("res://UI/Textures/Alpha.png") as Texture;
		Triangle = GD.Load("res://UI/Textures/Triangle.png") as Texture;
		NickLabelScene = GD.Load<PackedScene>("res://UI/NickLabel.tscn");
	}


	public void HotbarUpdate()
	{
		for(int Slot = 0; Slot <= 9; Slot++)
		{
			NinePatchRect SlotPatch = GetNode("CLayer/HotBarCenter/HBoxContainer/Vbox").GetChild(Slot) as NinePatchRect;
			if(Game.PossessedPlayer.Inventory[Slot] != null)
			{
				SlotPatch.Texture = Items.Thumbnail(Game.PossessedPlayer.Inventory[Slot].Type);
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


	public override void _Ready()
	{
		Crosshair = GetNode<TextureRect>("CLayer/CrossCenter/TextureRect");
		GetNode<Label>("CLayer/VersionLabel").Text = $"Version: {Game.Version}";

		GetTree().Connect("screen_resized", this, "OnScreenResized");
		HotbarUpdate();
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
			Vector3 PlayerPos = Game.PlayerList[Current.Key].Translation + new Vector3(0,7.5f,0);
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

		//TODO save these to private members in _Ready and clear up this mess
		((Label)(GetNode("CLayer").GetNode("FPSLabel"))).SetText(Engine.GetFramesPerSecond().ToString() + " fps");
		((Label)(GetNode("CLayer").GetNode("PlayerPosition"))).SetText("Player Position: " + Game.PossessedPlayer.Translation.Round().ToString());
		((Label)(GetNode("CLayer").GetNode("ChunkInfo"))).SetText("Current Chunk: (" + Game.PossessedPlayer.CurrentChunk.Item1 + ", 0, " + Game.PossessedPlayer.CurrentChunk.Item2 + ")");
	}
}
