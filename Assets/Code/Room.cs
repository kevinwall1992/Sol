using UnityEngine;
using System.Collections;

[ExecuteAlways]
[RequireComponent(typeof(SolidItemContainer))]
public class Room : Craft.Part
{
    public ItemContainer Container
    { get { return GetComponent<ItemContainer>(); } }
}
