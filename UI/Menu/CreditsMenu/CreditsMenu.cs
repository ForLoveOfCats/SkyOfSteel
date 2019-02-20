using Godot;


public class CreditsMenu : VBoxContainer
{
	private NinePatchRect CatLogo;
	private NinePatchRect BearLogo;

	public override void _Ready()
	{
		CatLogo = GetNode<NinePatchRect>("ForLoveOfCats/NinePatchRect");
		BearLogo = GetNode<NinePatchRect>("Stenodyon/NinePatchRect");
	}


	public override void _Process(float Delta)
	{
		CatLogo.RectMinSize = new Vector2(CatLogo.RectMinSize.x, CatLogo.RectSize.x);
		BearLogo.RectMinSize = new Vector2(BearLogo.RectMinSize.x, BearLogo.RectSize.x);
	}


	public void BackPressed()
	{
		Menu.BuildMain();
	}
}
