using Godot;


public interface IPushable {
	string Name { get; set; }
	Vector3 Translation { get; set; }

	void ApplyPush(Vector3 Push);
}

