using System.Collections.Generic;

public class ConduitNetwork<T>
{
    List<Conduit<T>> endpoints = new List<Conduit<T>>();

    public bool Push(T load)
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

    public void AddEndpoint(Conduit<T> endpoint)
    {
        endpoints.Add(endpoint);
    }

    public void RemoveEndpoint(Conduit<T> endpoint)
    {
        endpoints.Remove(endpoint);
    }
}