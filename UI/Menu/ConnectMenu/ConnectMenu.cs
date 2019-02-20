using Godot;
using System.Net;

public class ConnectMenu : VBoxContainer
{
	private Label AlertLabel;

	public override void _Ready()
	{
		AlertLabel = GetNode<Label>("AlertLabel");
	}


	public void ConnectPressed()
	{
		string Ip = GetNode<LineEdit>("HBoxContainer/IpEdit").GetText();

		if(Ip == "localhost")
		{
			Ip = "127.0.0.1";
		}

		IPAddress Address; //Unused, just to check if valid ip
		if(!IPAddress.TryParse(Ip, out Address))
		{
			//Invalid ip
			AlertLabel.Visible = true;
			AlertLabel.Text = "Please enter a valid IP address";
			return;
		}

		//Valid ip
		Net.ConnectTo(Ip);
	}


	public void BackPressed()
	{
		Menu.BuildMain();
	}
}
