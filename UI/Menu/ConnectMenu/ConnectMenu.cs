using Godot;
using System.Net;

public class ConnectMenu : VBoxContainer
{
	private Label AlertLabel;
	private LineEdit IpEdit;

	public override void _Ready()
	{
		AlertLabel = GetNode<Label>("AlertLabel");
		IpEdit = GetNode<LineEdit>("HBoxContainer/IpEdit");

		IpEdit.GrabFocus();

		Net.Self.Connect("ConnectToFailed", this, nameof(ConnectFailed));
	}


	public void ConnectFailed(string Ip)
	{
		//TODO Show warning to user that connection failed
	}


	public void EnterPressed(string Text)
	{
		ConnectPressed();
	}


	public void ConnectPressed()
	{
		string Ip = IpEdit.GetText();

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
