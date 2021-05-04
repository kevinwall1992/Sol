using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ledger<T>
{
    Dictionary<T, Manifest> manifests = 
        new Dictionary<T, Manifest>();

    public void Assign(T account, Item item, float quantity)
    {
        if (!manifests.ContainsKey(account))
            manifests[account] = new Manifest();

        manifests[account].Add(item, quantity);
    }

    public float Release(T account, Item item, float quantity = -1)
    {
        if (!manifests.ContainsKey(account))
            return 0;

        return manifests[account].Remove(item, quantity);
    }

    public void Transfer(T source, T destination, Item item, float quantity = -1)
    {
        Assign(source, item, Release(destination, item, quantity));
    }
}
