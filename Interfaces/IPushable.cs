using Godot;


public interface IPushable
{
	Vector3 Translation { get; set; }

	void ApplyPush(Vector3 Push);
}

