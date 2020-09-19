using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhysicalItem))]
public class VolumeItemQuantityStringController : MonoBehaviour
{
    public PhysicalItem PhysicalItem { get { return GetComponent<PhysicalItem>(); } }

    private void Start()
    {
        PhysicalItem.Item.GetQuantityString = delegate ()
        {
            float volume;
            string units;

            if (PhysicalItem.Volume < 1)
            {
                volume = PhysicalItem.Volume * 1000;
                units = "L";
            }
            else if (PhysicalItem.Volume < 1000)
            {
                volume = PhysicalItem.Volume;
                units = "kL";
            }
            else if (PhysicalItem.Volume < 1000000)
            {
                volume = PhysicalItem.Volume / 1000;
                units = "ML";
            }
            else
            {
                volume = PhysicalItem.Volume / 1000000;
                units = "GL";
            }

            switch (((int)volume).ToString().Length)
            {
                case 3: return ((int)volume).ToString() + units;
                case 2: return volume.ToString("F1") + units;
                case 1: return volume.ToString("F2") + units;
                case 0: return volume.ToString("F3") + units;

                default: return volume.ToString("E2");
            }
        };
    }
}
