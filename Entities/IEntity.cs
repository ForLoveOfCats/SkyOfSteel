using Godot;



public interface IEntity
{
	string Name { get; set; }
	bool Visible { get; set; }
	Vector3 Translation { get; set; }

	void Update(params object[] Args);
	void PhaseOut();
	void Destroy(params object[] Args);
}
