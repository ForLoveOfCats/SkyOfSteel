using Godot;

public interface Conduit<T>
{
    bool AttemptConnection(Vector3 from, ConduitNetwork<T> network);

    void Disconnect(Vector3 from, ConduitNetwork<T> network);

    bool PushToEndpoint(T load);
}