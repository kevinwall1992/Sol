using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room : Craft.Part
{
    public float Area;

    public float Volume { get { return Area * 2.4f; } }

    public Pocket RoomPocketPrefab;

    public override Manifest BoundItems
    {
        get
        {
            Manifest manifest = new Manifest();
            manifest[RoomPocketPrefab.Item] = 
                Volume / RoomPocketPrefab.Size;

            return manifest;
        }
    }


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
