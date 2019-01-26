using Godot;


public class HUD : Node
{
	private Texture Alpha;
	private Texture Triangle;

	HUD()
	{
		if(Engine.EditorHint) {return;}

		Alpha = GD.Load("res://UI/Alpha.png") as Texture;
		Triangle = GD.Load("res://UI/Triangle.png") as Texture;
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


	public override void _Ready()
	{
		GetTree().Connect("screen_resized", this, "OnScreenResized");
		HotbarUpdate();
	}


	public override void _Process(float Delta)
	{
		((Label)(GetNode("CLayer").GetNode("FPSLabel"))).SetText(Engine.GetFramesPerSecond().ToString() + " fps");
		((Label)(GetNode("CLayer").GetNode("PlayerPosition"))).SetText("Player Position: " + Game.PossessedPlayer.Translation.Round().ToString());
		((Label)(GetNode("CLayer").GetNode("ChunkInfo"))).SetText("Current Chunk: (" + Game.PossessedPlayer.CurrentChunk.Item1 + ", 0, " + Game.PossessedPlayer.CurrentChunk.Item2 + ")");
	}
}
