using UnityEngine;
using System.Collections;

public abstract class ComparableElement : UIElement
{
    public abstract System.IComparable Comparable { get; }
}