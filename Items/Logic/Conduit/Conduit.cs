using Godot;

public interface Conduit<T>
{
    /*
     * Attempts to connect this conduit to a network.
     * 
     * from: The position of the conduit that is attempting to conenct to
     * this one. Useful for buildings that may connect to different networks
     * on different sides.
     * 
     * network: the network to connect to.
     * 
     * returns a boolean value stating whether or not the connection attempt
     * was successful.
     */
    bool AttemptConnection(Vector3 from, ConduitNetwork<T> network);

    /*
     * Disconnects a conduit from the network.
     */
    void Disconnect(Vector3 from, ConduitNetwork<T> network);

    /*
     * Pushes a load to this endpoint. Called if this conduit is added to the network as an endpoint.
     */
    bool PushToEndpoint(T load);
}