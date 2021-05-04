using UnityEngine;
using System.Collections;

public class Commodity : Item.EquivalenceScript
{
    public override Vote IsEquivalent(Item other)
    {
        return (other.GetComponent<Commodity>() != null && 
                other.Name == Item.Name)
            .ToVote();
    }
}
