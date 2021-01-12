using UnityEngine;
using System.Collections;

[ExecuteAlways]
[RequireComponent(typeof(SolidItemContainer))]
public class Room : Craft.Part
{
    public float Area;

    public float Volume { get { return Area * Floor.CeilingHeight; } }

    public Ring.Floor Floor
    { get { return GetComponentInParent<Ring.Floor>(); } }

    public ItemContainer Container
    { get { return GetComponent<ItemContainer>(); } }


    public class Furniture : Item
    {
        public float InstallationLaborPerUnit;

        bool IsInstalled = false;

        public bool Install(Labor labor)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Status : MonoBehaviour
    {
        public class Accelerated : Status
        {
            public float Acceleration;
        }
    }
}
