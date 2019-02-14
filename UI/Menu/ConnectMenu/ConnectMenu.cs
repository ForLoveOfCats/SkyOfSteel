using Godot;
using System.Net;

public class ConnectMenu : VBoxContainer
{
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
