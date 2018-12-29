using Godot;
using System;
using System.Collections.Generic;
using Jurassic;


public class API : Node
{
	public enum LEVEL {CONSOLE, SERVER_GM, CLIENT_GM};


	static List<object> GetDelCall(string Name)
	{
		switch(Name)
		{
			case "print":
				return new List<object> {Name, new Action<string>(delegate(string ToPrint){Console.Print(ToPrint);})};

			case "log":
				return new List<object> {Name, new Action<string>(delegate(string ToLog){Console.Log(ToLog);})};

			case "host":
				return new List<object> {Name, new Action(delegate(){
					Net.Host();
				})};

			case "connect":
				return new List<object> {Name, new Action<string>(delegate(string Ip){
					if(Ip == "" || Ip == "localhost" || Ip == "undefined")
					{
						Ip = "127.0.0.1";
					}
					Net.ConnectTo(Ip);
				})};

			case "disconnect":
				return new List<object> {Name, new Action(delegate(){
					Net.Disconnect();
				})};

			case "ms_get":
				return new List<object> {Name, new Func<int>(() => {return OS.GetTicksMsec();})};

			case "peerlist_get":
				return new List<object> {Name, new Func<Jurassic.Library.ArrayInstance>(() => {
					Jurassic.Library.ArrayInstance Out = Scripting.ConsoleEngine.Array.Construct();
					foreach(int Id in Net.PeerList)
					{
						Out.Push(Id);
					}
					return Out;
				})};

			case "bind":
				return new List<object> {Name, new Action<string, string>(delegate(string FunctionName, string InputString){
					Bindings.Bind(FunctionName, InputString);
				})};

			case "unbind":
				return new List<object> {Name, new Action<string>(delegate(string FunctionName){
					Bindings.UnBind(FunctionName);
				})};

			case "player_input_forward_set":
				return new List<object> {Name, new Action<double>(delegate(double Sens){
					Game.PossessedPlayer.ForwardMove(Sens);
				})};

			case "player_input_forward_get":
				return new List<object> {Name, new Func<double>(() => {return Game.PossessedPlayer.ForwardSens;})};

			case "player_input_backward_set":
				return new List<object> {Name, new Action<double>(delegate(double Sens){
					Game.PossessedPlayer.BackwardMove(Sens);
				})};

			case "player_input_backward_get":
				return new List<object> {Name, new Func<double>(() => {return Game.PossessedPlayer.BackwardSens;})};

			case "player_input_right_set":
				return new List<object> {Name, new Action<double>(delegate(double Sens){
					Game.PossessedPlayer.RightMove(Sens);
				})};

			case "player_input_right_get":
				return new List<object> {Name, new Func<double>(() => {return Game.PossessedPlayer.RightSens;})};

			case "player_input_left_set":
				return new List<object> {Name, new Action<double>(delegate(double Sens){
					Game.PossessedPlayer.LeftMove(Sens);
				})};

			case "player_input_left_get":
				return new List<object> {Name, new Func<double>(() => {return Game.PossessedPlayer.LeftSens;})};

			case "player_input_sprint_set":
				return new List<object> {Name, new Action<double>(delegate(double Sens){
					Game.PossessedPlayer.Sprint(Sens);
				})};

			case "player_input_sprint_get":
				return new List<object> {Name, new Func<double>(() => {return Game.PossessedPlayer.IsSprinting ? 1d : 0d;})};

			case "player_input_jump_set":
				return new List<object> {Name, new Action<double>(delegate(double Sens){
					Game.PossessedPlayer.Jump(Sens);
				})};

			case "player_input_jump_get":
				return new List<object> {Name, new Func<double>(() => {return Game.PossessedPlayer.IsJumping ? 1d : 0d;})};

			case "player_input_inventory_up":
				return new List<object> {Name, new Action(delegate(){
					Game.PossessedPlayer.InventoryUp();
				})};

			case "player_input_inventory_down":
				return new List<object> {Name, new Action(delegate(){
					Game.PossessedPlayer.InventoryDown();
				})};

			case "player_input_look_up":
				return new List<object> {Name, new Action<double>(delegate(double Sens){
					Game.PossessedPlayer.LookUp(Sens);
				})};

			case "player_input_look_down":
				return new List<object> {Name, new Action<double>(delegate(double Sens){
					Game.PossessedPlayer.LookDown(Sens);
				})};

			case "player_input_look_right":
				return new List<object> {Name, new Action<double>(delegate(double Sens){
					Game.PossessedPlayer.LookRight(Sens);
				})};

			case "player_input_look_left":
				return new List<object> {Name, new Action<double>(delegate(double Sens){
					Game.PossessedPlayer.LookLeft(Sens);
				})};

			case "player_input_primary_fire":
				return new List<object> {Name, new Action<double>(delegate(double Sens){
					Game.PossessedPlayer.PrimaryFire(Sens);
				})};

			case "player_input_secondary_fire":
				return new List<object> {Name, new Action<double>(delegate(double Sens){
					Game.PossessedPlayer.SecondaryFire(Sens);
				})};

			case "gamemode_set":
				return new List<object> {Name, new Action<string>(delegate(string GameModeName){
					if(Game.Self.GetTree().GetNetworkPeer() != null && Game.Self.GetTree().GetNetworkUniqueId() == 1)
					{
						Scripting.LoadGameMode(GameModeName);
					}
					else
					{
						Console.Print("Error: Cannot set gamemode as client");
					}
				})};

			case "gamemode_get":
				return new List<object> {Name, new Func<string>(() => {return Scripting.GamemodeName;})};

			case "chunk_render_distance_set":
				return new List<object> {Name, new Action<double>(delegate(double Distance){
					if(Distance < 2d)
					{
						Console.Print("Cannot set render distance value lower than two chunks");
						return;
					}
					Game.ChunkRenderDistance = (int)Distance;
					Game.PossessedPlayer.UnloadAndRequestChunks();
				})};

			case "chunk_render_distance_get":
				return new List<object> {Name, new Func<double>(() => {return Convert.ToDouble(Game.ChunkRenderDistance);})};

			case "save":
				return new List<object> {Name, new Action(delegate(){
					Building.SaveWorld("TestSave");
				})};

			case "load":
				return new List<object> {Name, new Action(delegate(){
					Building.LoadWorld("TestSave");
				})};

			default:
				throw new System.ArgumentException("Invalid GetDelCall name arg '" + Name + "'");
		}
	}


	public static List<List<object>> Expose(LEVEL ApiLevel, Scripting ScriptingRef)
	{
		List<List<object>> Output = new List<List<object>>();

		switch(ApiLevel)
		{
			case LEVEL.CONSOLE:
				Output.Add(GetDelCall("print"));
				Output.Add(GetDelCall("log"));
				Output.Add(GetDelCall("ms_get"));
				Output.Add(GetDelCall("host"));
				Output.Add(GetDelCall("connect"));
				Output.Add(GetDelCall("disconnect"));
				Output.Add(GetDelCall("peerlist_get"));
				Output.Add(GetDelCall("bind"));
				Output.Add(GetDelCall("unbind"));
				Output.Add(GetDelCall("player_input_forward_set"));
				Output.Add(GetDelCall("player_input_forward_get"));
				Output.Add(GetDelCall("player_input_backward_set"));
				Output.Add(GetDelCall("player_input_backward_get"));
				Output.Add(GetDelCall("player_input_right_set"));
				Output.Add(GetDelCall("player_input_right_get"));
				Output.Add(GetDelCall("player_input_left_set"));
				Output.Add(GetDelCall("player_input_left_get"));
				Output.Add(GetDelCall("player_input_sprint_set"));
				Output.Add(GetDelCall("player_input_sprint_get"));
				Output.Add(GetDelCall("player_input_jump_set"));
				Output.Add(GetDelCall("player_input_jump_get"));
				Output.Add(GetDelCall("player_input_inventory_up"));
				Output.Add(GetDelCall("player_input_inventory_down"));
				Output.Add(GetDelCall("player_input_look_up"));
				Output.Add(GetDelCall("player_input_look_down"));
				Output.Add(GetDelCall("player_input_look_right"));
				Output.Add(GetDelCall("player_input_look_left"));
				Output.Add(GetDelCall("player_input_primary_fire"));
				Output.Add(GetDelCall("player_input_secondary_fire"));
				Output.Add(GetDelCall("gamemode_set"));
				Output.Add(GetDelCall("gamemode_get"));
				Output.Add(GetDelCall("chunk_render_distance_set"));
				Output.Add(GetDelCall("chunk_render_distance_get"));
				Output.Add(GetDelCall("save"));
				Output.Add(GetDelCall("load"));
				break;
			case LEVEL.SERVER_GM:
				Output.Add(GetDelCall("log"));
				Output.Add(GetDelCall("ms_get"));
				Output.Add(GetDelCall("peerlist_get"));
				break;
			case LEVEL.CLIENT_GM:
				Output.Add(GetDelCall("log"));
				Output.Add(GetDelCall("ms_get"));
				Output.Add(GetDelCall("peerlist_get"));
				break;
		}

		return Output;
	}
}
