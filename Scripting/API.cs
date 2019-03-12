using Godot;
using System;
using System.Collections.Generic;


public class API : Node
{
	public class PyConstructorExposer
	{
		public string Name = null;
		public Delegate Constructor = null;

		public PyConstructorExposer()
		{}

		public PyConstructorExposer(string NameArg, Delegate ConArg)
		{
			Name = NameArg;
			Constructor = ConArg;
		}
	}


	public enum LEVEL {CONSOLE, GAMEMODE};


	static List<object> GetDelCall(string Name)
	{
		switch(Name)
		{
			case "quit":
				return new List<object> {Name, new Action(delegate(){
					Game.Quit();
				})};

			case "printf":
				return new List<object> {Name, new Action<object>(delegate(object ToPrint){Console.Print(Scripting.PyToString(ToPrint));})};

			case "logf":
				return new List<object> {Name, new Action<object>(delegate(object ToLog){Console.Log(Scripting.PyToString(ToLog));})};

			case "host":
				return new List<object> {Name, new Action(delegate(){
					if(Game.Nickname == "")
					{
						Console.ThrowPrint("Please set a multiplayer nickname before hosting");
					}
					else
					{
						Net.Host();
					}
				})};

			case "connect":
				return new List<object> {Name, new Action<string>(delegate(string Ip){
					if(Game.Nickname == "")
					{
						Console.ThrowPrint("Please set a multiplayer nickname before connecting");
					}
					else
					{
						if(Ip == "" || Ip == "localhost" || Ip == "undefined")
						{
							Ip = "127.0.0.1";
						}
						Net.ConnectTo(Ip);
					}
				})};

			case "nickname":
				return new List<object> {Name, new Action<string>(delegate(string NickArg){
					if(Game.WorldOpen)
					{
						Console.ThrowPrint("Cannot set nickname while hosting or connected");
						return;
					}
					Game.Nickname = NickArg;
				})};

			case "nickname_get":
				return new List<object> {Name, new Func<string>(() => {return Game.Nickname;})};

			case "remote_nickname_get":
				return new List<object> {Name, new Func<float, string>((float Id) => {
					string Nick;
					if(Net.Nicknames.TryGetValue((int)Id, out Nick))
					{
						return Nick;
					}
					return "";
				})};

			case "disconnect":
				return new List<object> {Name, new Action(delegate(){
					if(!Game.WorldOpen)
					{
						Console.ThrowPrint("Neither connected nor hosting");
						return;
					}
					Net.Disconnect();
				})};

			case "ms_get":
				return new List<object> {Name, new Func<int>(() => {return OS.GetTicksMsec();})};

			case "peerlist_get":
				return new List<object> {Name, new Func<IronPython.Runtime.List>(() => {
					IronPython.Runtime.List Out = new IronPython.Runtime.List();
					foreach(int Id in Net.PeerList)
					{
						Out.append(Id);
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

			case "player_input_forward":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.ForwardMove(Sens);
				})};

			case "player_input_forward_get":
				return new List<object> {Name, new Func<float>(() => {return Game.PossessedPlayer.ForwardSens;})};

			case "player_input_backward":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.BackwardMove(Sens);
				})};

			case "player_input_backward_get":
				return new List<object> {Name, new Func<float>(() => {return Game.PossessedPlayer.BackwardSens;})};

			case "player_input_right":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.RightMove(Sens);
				})};

			case "player_input_right_get":
				return new List<object> {Name, new Func<float>(() => {return Game.PossessedPlayer.RightSens;})};

			case "player_input_left":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.LeftMove(Sens);
				})};

			case "player_input_left_get":
				return new List<object> {Name, new Func<float>(() => {return Game.PossessedPlayer.LeftSens;})};

			case "player_input_sprint":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.Sprint(Sens);
				})};

			case "player_input_sprint_get":
				return new List<object> {Name, new Func<float>(() => {return Game.PossessedPlayer.IsSprinting ? 1 : 0;})};

			case "player_input_jump":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.Jump(Sens);
				})};

			case "player_input_jump_get":
				return new List<object> {Name, new Func<float>(() => {return Game.PossessedPlayer.IsJumping ? 1 : 0;})};

			case "player_input_crouch":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.Crouch(Sens);
				})};

			case "player_input_crouch_get":
				return new List<object> {Name, new Func<float>(() => {return Game.PossessedPlayer.IsCrouching ? 1 : 0;})};

			case "player_input_inventory_up":
				return new List<object> {Name, new Action(delegate(){
					Game.PossessedPlayer.InventoryUp();
				})};

			case "player_input_inventory_down":
				return new List<object> {Name, new Action(delegate(){
					Game.PossessedPlayer.InventoryDown();
				})};

			case "player_input_look_up":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.LookUp(Sens);
				})};

			case "player_input_look_down":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.LookDown(Sens);
				})};

			case "player_input_look_right":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.LookRight(Sens);
				})};

			case "player_input_look_left":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.LookLeft(Sens);
				})};

			case "player_input_build_rotate":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.BuildRotate(Sens);
				})};

			case "player_input_primary_fire":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.PrimaryFire(Sens);
				})};

			case "player_input_secondary_fire":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.SecondaryFire(Sens);
				})};

			case "player_input_drop":
				return new List<object> {Name, new Action<float>(delegate(float Sens){
					Game.PossessedPlayer.DropCurrentItem(Sens);
				})};

			case "player_position_reset":
				return new List<object> {Name, new Action(delegate(){
					Game.PossessedPlayer.PositionReset();
				})};

			case "fly":
				return new List<object> {Name, new Action<bool>(delegate(bool NewFly){
					Game.PossessedPlayer.SetFly(NewFly);
				})};

			case "fly_toggle":
				return new List<object> {Name, new Action(delegate(){
					Game.PossessedPlayer.SetFly(!Game.PossessedPlayer.FlyMode);
				})};

			case "fly_get":
				return new List<object> {Name, new Func<bool>(() => {return Game.PossessedPlayer.FlyMode;})};

			case "gamemode":
				return new List<object> {Name, new Action<string>(delegate(string GameModeName){
					if(Game.Self.GetTree().GetNetworkPeer() != null && Game.Self.GetTree().GetNetworkUniqueId() == 1)
					{
						Scripting.LoadGameMode(GameModeName);
					}
					else
					{
						Console.ThrowPrint("Cannot set gamemode as client");
					}
				})};

			case "gamemode_get":
				return new List<object> {Name, new Func<string>(() => {return Scripting.GamemodeName;})};

			case "reload":
				return new List<object> {Name, new Action(delegate(){
					if(Game.Self.GetTree().GetNetworkPeer() != null && Game.Self.GetTree().GetNetworkUniqueId() == 1)
					{
						Scripting.LoadGameMode(Scripting.GamemodeName);
					}
					else
					{
						Console.ThrowPrint("Cannot reload gamemode as client");
					}
				})};

			case "fps_get":
				return new List<object> {Name, new Func<float>(() => {return Engine.GetFramesPerSecond();})};

			case "fps_max":
				return new List<object> {Name, new Action<float>(delegate(float TargetFps){
					if(TargetFps <= 1)
					{
						Console.ThrowPrint($"Please provide a valid fps value which is greater than 1");
						return;
					}
					Engine.SetTargetFps(Convert.ToInt32(TargetFps));
				})};

			case "fps_max_get":
				return new List<object> {Name, new Func<float>(() => {return Engine.GetTargetFps();})};

			case "chunk_render_distance":
				return new List<object> {Name, new Action<float>(delegate(float Distance){
					if(Distance < 2)
					{
						Console.ThrowPrint("Cannot set render distance value lower than two chunks");
						return;
					}
					Game.ChunkRenderDistance = (int)Distance;
					Net.UnloadAndRequestChunks();
				})};

			case "chunk_render_distance_get":
				return new List<object> {Name, new Func<float>(() => {return Game.ChunkRenderDistance;})};

			case "save":
				return new List<object> {Name, new Action<string>(delegate(string SaveName){
					if(!Game.WorldOpen)
					{
						Console.ThrowPrint("Cannot save world when not hosting");
						return;
					}

					if(SaveName == "undefined")
					{
						Console.ThrowPrint("Please provide a name to save under");
						return;
					}

					Game.SaveWorld(SaveName);
					Console.Print($"Saved world to save '{SaveName}' successfully");
				})};

			case "load":
				return new List<object> {Name, new Action<string>(delegate(string SaveName){
					if(!Game.WorldOpen)
					{
						Console.ThrowPrint("Cannot load savegame when not hosting");
						return;
					}

					if(SaveName == "undefined")
					{
						Console.ThrowPrint("Please provide the name of a save to load");
						return;
					}

					if(Game.LoadWorld(SaveName))
					{
						Console.Print($"Loaded save '{SaveName}' successfully");
					}
					else
					{
						Console.Print($"Failed to load save '{SaveName}");
					}
				})};

			case "hud_hide": //TODO error check this
				return new List<object> {Name, new Action(delegate(){
					Game.PossessedPlayer.HUDInstance.Hide();
				})};

			case "hud_show": //TODO error check this
				return new List<object> {Name, new Action(delegate(){
					Game.PossessedPlayer.HUDInstance.Show();
				})};

			case "type_get":
				return new List<object> {Name, new Func<object, string>((Obj) => {return Obj.GetType().ToString();})};

			default:
				throw new System.ArgumentException("Invalid GetDelCall name arg '" + Name + "'");
		}
	}


	public static PyConstructorExposer GetConstructor(string Name)
	{
		switch(Name)
		{
			case "Vector3":
				return new PyConstructorExposer(Name, new Func<float, float, float, PyVector3>((X, Y, Z) => {return new PyVector3(X, Y, Z);}));

			default:
				throw new System.ArgumentException("Invalid GetConstructor name arg '" + Name + "'");
		}
	}


	public static List<List<object>> Expose(LEVEL ApiLevel, Scripting ScriptingRef)
	{
		List<List<object>> Output = new List<List<object>>();

		switch(ApiLevel)
		{
			case LEVEL.CONSOLE:
				Output.Add(GetDelCall("quit"));
				Output.Add(GetDelCall("printf"));
				Output.Add(GetDelCall("logf"));
				Output.Add(GetDelCall("ms_get"));
				Output.Add(GetDelCall("host"));
				Output.Add(GetDelCall("connect"));
				Output.Add(GetDelCall("nickname"));
				Output.Add(GetDelCall("nickname_get"));
				Output.Add(GetDelCall("remote_nickname_get"));
				Output.Add(GetDelCall("disconnect"));
				Output.Add(GetDelCall("peerlist_get"));
				Output.Add(GetDelCall("bind"));
				Output.Add(GetDelCall("unbind"));
				Output.Add(GetDelCall("player_input_forward"));
				Output.Add(GetDelCall("player_input_forward_get"));
				Output.Add(GetDelCall("player_input_backward"));
				Output.Add(GetDelCall("player_input_backward_get"));
				Output.Add(GetDelCall("player_input_right"));
				Output.Add(GetDelCall("player_input_right_get"));
				Output.Add(GetDelCall("player_input_left"));
				Output.Add(GetDelCall("player_input_left_get"));
				Output.Add(GetDelCall("player_input_sprint"));
				Output.Add(GetDelCall("player_input_sprint_get"));
				Output.Add(GetDelCall("player_input_jump"));
				Output.Add(GetDelCall("player_input_jump_get"));
				Output.Add(GetDelCall("player_input_crouch"));
				Output.Add(GetDelCall("player_input_crouch_get"));
				Output.Add(GetDelCall("player_input_inventory_up"));
				Output.Add(GetDelCall("player_input_inventory_down"));
				Output.Add(GetDelCall("player_input_look_up"));
				Output.Add(GetDelCall("player_input_look_down"));
				Output.Add(GetDelCall("player_input_look_right"));
				Output.Add(GetDelCall("player_input_look_left"));
				Output.Add(GetDelCall("player_input_build_rotate"));
				Output.Add(GetDelCall("player_input_primary_fire"));
				Output.Add(GetDelCall("player_input_secondary_fire"));
				Output.Add(GetDelCall("player_input_drop"));
				Output.Add(GetDelCall("player_position_reset"));
				Output.Add(GetDelCall("gamemode"));
				Output.Add(GetDelCall("gamemode_get"));
				Output.Add(GetDelCall("reload"));
				Output.Add(GetDelCall("chunk_render_distance"));
				Output.Add(GetDelCall("chunk_render_distance_get"));
				Output.Add(GetDelCall("fps_get"));
				Output.Add(GetDelCall("fps_max"));
				Output.Add(GetDelCall("fps_max_get"));
				Output.Add(GetDelCall("save"));
				Output.Add(GetDelCall("load"));
				Output.Add(GetDelCall("hud_hide"));
				Output.Add(GetDelCall("hud_show"));
				Output.Add(GetDelCall("fly"));
				Output.Add(GetDelCall("fly_toggle"));
				Output.Add(GetDelCall("fly_get"));
				Output.Add(GetDelCall("type_get"));
				break;
			case LEVEL.GAMEMODE:
				Output.Add(GetDelCall("printf"));
				Output.Add(GetDelCall("logf"));
				Output.Add(GetDelCall("ms_get"));
				Output.Add(GetDelCall("nickname_get"));
				Output.Add(GetDelCall("remote_nickname_get"));
				Output.Add(GetDelCall("peerlist_get"));
				Output.Add(GetDelCall("player_input_forward"));
				Output.Add(GetDelCall("player_input_forward_get"));
				Output.Add(GetDelCall("player_input_backward"));
				Output.Add(GetDelCall("player_input_backward_get"));
				Output.Add(GetDelCall("player_input_right"));
				Output.Add(GetDelCall("player_input_right_get"));
				Output.Add(GetDelCall("player_input_left"));
				Output.Add(GetDelCall("player_input_left_get"));
				Output.Add(GetDelCall("player_input_sprint"));
				Output.Add(GetDelCall("player_input_sprint_get"));
				Output.Add(GetDelCall("player_input_jump"));
				Output.Add(GetDelCall("player_input_jump_get"));
				Output.Add(GetDelCall("player_input_crouch"));
				Output.Add(GetDelCall("player_input_crouch_get"));
				Output.Add(GetDelCall("player_input_inventory_up"));
				Output.Add(GetDelCall("player_input_inventory_down"));
				Output.Add(GetDelCall("player_input_look_up"));
				Output.Add(GetDelCall("player_input_look_down"));
				Output.Add(GetDelCall("player_input_look_right"));
				Output.Add(GetDelCall("player_input_look_left"));
				Output.Add(GetDelCall("player_input_build_rotate"));
				Output.Add(GetDelCall("player_input_primary_fire"));
				Output.Add(GetDelCall("player_input_secondary_fire"));
				Output.Add(GetDelCall("player_position_reset"));
				Output.Add(GetDelCall("gamemode_get"));
				Output.Add(GetDelCall("chunk_render_distance"));
				Output.Add(GetDelCall("chunk_render_distance_get"));
				Output.Add(GetDelCall("fps_get"));
				Output.Add(GetDelCall("fps_max_get"));
				Output.Add(GetDelCall("save"));
				Output.Add(GetDelCall("load"));
				Output.Add(GetDelCall("hud_hide"));
				Output.Add(GetDelCall("hud_show"));
				Output.Add(GetDelCall("fly"));
				Output.Add(GetDelCall("fly_toggle"));
				Output.Add(GetDelCall("fly_get"));
				Output.Add(GetDelCall("type_get"));
				break;
		}

		return Output;
	}

	public static List<PyConstructorExposer> ExposeConstructors(LEVEL ApiLevel)
	{
		List<PyConstructorExposer> Output = new List<PyConstructorExposer>();

		switch(ApiLevel)
		{
			case LEVEL.CONSOLE:
				Output.Add(GetConstructor("Vector3"));
				break;
			case LEVEL.GAMEMODE:
				Output.Add(GetConstructor("Vector3"));
				break;
		}

		return Output;
	}
}
