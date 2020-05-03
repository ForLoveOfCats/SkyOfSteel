using Godot;
using System.Collections.Generic;


public class HUD : Node {
	public static float DamageIndicatorLifeMultiplyer = 0.1f; //Multipled by damage to calc max life

	private Texture Alpha;
	private Texture Triangle;
	private PackedScene ItemCountLabelScene;
	private PackedScene NickLabelScene;
	private PackedScene DamageIndicatorScene;

	private Dictionary<int, Label> NickLabels = new Dictionary<int, Label>();

	private TextureRect Crosshair;
	private ProgressBar CooldownBar;
	private ProgressBar HealthBar;
	private Label ChunkInfoLabel;
	private Label PlayerPositionLabel;
	private Label FPSLabel;
	private Node2D DamageIndicatorRoot;
	public CanvasLayer NickLabelLayer;

	public bool Visible = true;

	HUD() {
		if(Engine.EditorHint) { return; }

		Alpha = GD.Load("res://UI/Textures/Alpha.png") as Texture;
		Triangle = GD.Load("res://UI/Textures/Triangle.png") as Texture;
		ItemCountLabelScene = GD.Load<PackedScene>("res://UI/ItemCountLabel.tscn");
		NickLabelScene = GD.Load<PackedScene>("res://UI/NickLabel.tscn");
		DamageIndicatorScene = GD.Load<PackedScene>("res://UI/DamageIndicator.tscn");
	}


	public override void _Ready() {
		Crosshair = GetNode<TextureRect>("CLayer/CrossCenter/TextureRect");
		CooldownBar = GetNode<ProgressBar>("CLayer/CooldownCenter/VBox/CooldownBar");
		HealthBar = GetNode<ProgressBar>("CLayer/HealthVBox/HealthHBox/HealthBar");
		ChunkInfoLabel = GetNode<Label>("CLayer/ChunkInfo");
		PlayerPositionLabel = GetNode<Label>("CLayer/PlayerPosition");
		FPSLabel = GetNode<Label>("CLayer/FPSLabel");
		DamageIndicatorRoot = GetNode<Node2D>("CLayer/DamageIndicatorRoot");
		NickLabelLayer = GetNode<CanvasLayer>("NickLabelLayer");

		GetNode<Label>("CLayer/VersionLabel").Text = $"Version: {Game.Version}";

		GetTree().Connect("screen_resized", this, nameof(OnScreenResized));
		HotbarUpdate();
		CallDeferred(nameof(OnScreenResized));

		Show(); //To make sure we catch anything which might be wrong after hide then show
	}


	public void HotbarUpdate() {
		Game.PossessedPlayer.MatchSome(
			(Plr) => {
				for(int Slot = 0; Slot <= 9; Slot++) {
					var SlotPatch = GetNode("CLayer/HotBarCenter/HBoxContainer/Vbox").GetChild<NinePatchRect>(Slot);
					if(Plr.Inventory[Slot] != null) {
						SlotPatch.Texture = Items.Thumbnails[Plr.Inventory[Slot].Id];

						foreach(Node Child in SlotPatch.GetChildren())
							Child.QueueFree();

						var CountLabel = (Label)ItemCountLabelScene.Instance();
						CountLabel.Text = Plr.Inventory[Slot].Count.ToString();
						SlotPatch.AddChild(CountLabel);
					}
					else {
						SlotPatch.Texture = Alpha;

						foreach(Node Child in SlotPatch.GetChildren()) {
							Child.QueueFree();
						}
					}

					SlotPatch.RectMinSize = new Vector2(GetViewport().Size.y / 11, GetViewport().Size.y / 11);

					var ActiveIndicatorPatch = GetNode("CLayer/HotBarCenter/HBoxContainer/Vbox2").GetChild<NinePatchRect>(Slot);
					ActiveIndicatorPatch.RectMinSize = new Vector2(GetViewport().Size.y / 11, GetViewport().Size.y / 11);
					ActiveIndicatorPatch.Texture = Alpha;
				}

				((NinePatchRect)(GetNode("CLayer/HotBarCenter/HBoxContainer/Vbox2").GetChild(Plr.InventorySlot))).Texture = Triangle;
			}
		);
	}


	public void OnScreenResized() //Not an override
	{
		HotbarUpdate();
	}


	private void HideNodes(Godot.Collections.Array Nodes) {
		foreach(Node ToHide in Nodes) {
			if(ToHide is CanvasItem) {
				((CanvasItem)ToHide).Hide();
			}
			HideNodes(ToHide.GetChildren());
		}
	}


	public void Hide() {
		HideNodes(GetChildren());
		Visible = false;
	}


	private void ShowNodes(Godot.Collections.Array Nodes) {
		foreach(Node ToShow in Nodes) {
			if(ToShow is CanvasItem) {
				((CanvasItem)ToShow).Show();
			}
			ShowNodes(ToShow.GetChildren());
		}
	}


	public void Show() {
		ShowNodes(GetChildren());
		Visible = true;
		CallDeferred(nameof(HotbarUpdate));
	}


	public void AddNickLabel(int Id, string Nick) {
		var Instance = (Label)NickLabelScene.Instance();
		Instance.Text = Nick;
		NickLabelLayer.AddChild(Instance);
		NickLabels[Id] = Instance;
	}


	public void RemoveNickLabel(int Id) {
		if(NickLabels.ContainsKey(Id)) {
			NickLabels[Id].QueueFree();
			NickLabels.Remove(Id);
		}
	}


	public void AddDamageIndicator(Vector3 ShotFirePosition, float Damage) {
		var Indicator = (DamageIndicator)DamageIndicatorScene.Instance();
		Indicator.Setup(ShotFirePosition, Damage * DamageIndicatorLifeMultiplyer);
		DamageIndicatorRoot.AddChild(Indicator);
	}


	public void ClearDamageIndicators() {
		foreach(Node Child in DamageIndicatorRoot.GetChildren())
			Child.QueueFree();
	}


	public override void _Process(float Delta) {
		Game.PossessedPlayer.Match(
			none: () => Hide(),

			some: (Plr) => {
				Crosshair.Visible = !Menu.IsOpen;
				CooldownBar.Visible = !Menu.IsOpen;

				CooldownBar.MaxValue = Plr.CurrentMaxCooldown;
				CooldownBar.Value = Plr.CurrentCooldown;

				HealthBar.MaxValue = Player.MaxHealth;
				HealthBar.Value = Plr.Health;

				HotbarUpdate(); //TODO: Use InventoryIcon

				foreach(KeyValuePair<int, Label> Current in NickLabels) {
					Net.Players[Current.Key].Plr.Match(
						none: () => Current.Value.Visible = false,

						some: (OwningPlayer) => {
							//On the server when a player instance is unloaded due to render distance it technically still exists
							//So as precaution if the nametag's player is outside the render distance we hide it
							var OwningPlayerChunk = World.GetChunkTuple(OwningPlayer.Translation);
							if(!World.ChunkWithinDistanceFrom(OwningPlayerChunk, Game.ChunkRenderDistance, Plr.Translation)) {
								Current.Value.Visible = false;
								return; //continue foreach by exiting lambda
							}

							Vector3 PlayerPos = OwningPlayer.Translation + new Vector3(0, 7.5f, 0);
							if(Net.Players[OwningPlayer.Id].Team != Net.Players[Plr.Id].Team || Plr.Cam.IsPositionBehind(PlayerPos)) {
								Current.Value.Visible = false;
							}
							else {
								Current.Value.Visible = Visible;
								Current.Value.MarginLeft = Plr.Cam.UnprojectPosition(PlayerPos).x - Current.Value.RectSize.x / 2;
								Current.Value.MarginTop = Plr.Cam.UnprojectPosition(PlayerPos).y - Current.Value.RectSize.y / 2;
							}
						}
					);
				}

				ChunkInfoLabel.Text = $"Current Chunk: ({Plr.DepreciatedCurrentChunk.Item1}, 0, {Plr.DepreciatedCurrentChunk.Item2})";
				PlayerPositionLabel.Text = $"Player Position: {Plr.Translation.Round()}";
				FPSLabel.Text = $"{Engine.GetFramesPerSecond()} fps";
			}
		);
	}
}
