using System.Collections.Generic;

public class ConduitNetwork<T>
{
    List<Conduit<T>> endpoints = new List<Conduit<T>>();

    /*
     * Attempts to push something to an endpoint in the network and returns true upon success or
     * returns false upon failure.
     */
    public bool AttemptPush(T load)
    {
        if (endpoints.Count == 0)
        {
            return false;
        }

        foreach (Conduit<T> endpoint in endpoints)
        {
            if (endpoint.PushToEndpoint(load) == true)
            {
                return true;
            }
        }

        return false;
    }

    /*
     * Adds a conduit as an endpoint to the network.
     */
    public void AddEndpoint(Conduit<T> endpoint)
    {
        endpoints.Add(endpoint);
    }

    /*
     * Removes an endpoint from the network.
     */
    public void RemoveEndpoint(Conduit<T> endpoint)
    {
        endpoints.Remove(endpoint);
    }
}