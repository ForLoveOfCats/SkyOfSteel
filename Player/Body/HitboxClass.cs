using Godot;


public class HitboxClass : StaticBody {
	[Export] public TYPE Type = TYPE.BODY;

	public Player OwningPlayer = null;


	public enum TYPE { HEAD, BODY, LEGS }
}
