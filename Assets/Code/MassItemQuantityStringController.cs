using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhysicalItem))]
public class MassItemQuantityStringController : MonoBehaviour
{
    public PhysicalItem PhysicalItem { get { return GetComponent<PhysicalItem>(); } }

    private void Start()
    {
        PhysicalItem.Item.GetQuantityString = delegate ()
        {
            float mass;
            string units;

            if (PhysicalItem.Mass < 1000)
            {
                mass = PhysicalItem.Mass;
                units = "kg";
            }
            else if (PhysicalItem.Mass < 1000000)
            {
                mass = PhysicalItem.Mass / 1000;
                units = "t";
            }
            else if (PhysicalItem.Mass < 1000000000)
            {
                mass = PhysicalItem.Mass / 1000000;
                units = "kt";
            }
            else
            {
                mass = PhysicalItem.Mass / 1000000000;
                units = "Mt";
            }

            return mass.ToShortString() + units;
        };
    }
}
