using Godot;


public class ShaderCompilerHider : MeshInstance {
	public bool LivedOneFrame;


	public override void _Process(float Delta) {
		if(LivedOneFrame) {
			QueueFree();
		}

		LivedOneFrame = true;
	}
}
