using Godot;



public interface IEntity
{
	Vector3 Translation { get; set; }

	void Update(params object[] Args);
	void Destroy(params object[] Args);
}
