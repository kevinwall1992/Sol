using UnityEngine;
using System.Collections;

public class Pocket : Item.Script
{
    public PocketType Type = PocketType.Solid;
    public float Size;

    public static float GetUnitSize(Item item)
    {
        switch(item.Type)
        {
            case PocketType.Solid:
            case PocketType.Liquid:
            case PocketType.Cryo:
            case PocketType.Living:
                return item.GetVolumePerUnit();

            case PocketType.Pocket:
                return 0;

            case PocketType.Labor:
                throw new System.NotImplementedException();

            default:
                return 0;
        }
    }
}

public enum PocketType { None, Solid, Liquid, Cryo, Living, Labor, Pocket }
