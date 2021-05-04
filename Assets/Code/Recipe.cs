using UnityEngine;
using System.Collections;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public class Recipe
{
    public EditorManifest Inputs;

    public Item Output;
    public float OutputQuantity;

    public float Work;
}
