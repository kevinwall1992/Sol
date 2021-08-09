using UnityEngine;
using System.Collections;

public class CraftStorage : Craft.Part
{
    public Pocket PocketPrefab;
    public float CubicMetersPerUnit = 1;

    public override Manifest BoundItems
    {
        get
        {
            Manifest manifest = new Manifest();

            float pocket_quantity = Item.Quantity * CubicMetersPerUnit / PocketPrefab.Size;
            manifest.Add(PocketPrefab.Item, pocket_quantity);

            return manifest;
        }
    }
}
