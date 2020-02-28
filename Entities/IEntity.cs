using Godot;



public interface IEntity
{
	void Update(params object[] Args);
	void Destroy(params object[] Args);
}
