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

            switch (((int)mass).ToString().Length)
            {
                case 3: return ((int)mass).ToString() + units;
                case 2: return mass.ToString("F1") + units;
                case 1: return mass.ToString("F2") + units;
                case 0: return mass.ToString("F3") + units;

                default: return mass.ToString("E2");
            }
        };
    }
}
