﻿using UnityEngine;
using System.Collections;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public class Recipe
{
    public InputDictionary Inputs;

    public Item Output;
    public float OutputQuantity;

    public float Work;

    [System.Serializable]
    public class InputDictionary : SerializableDictionaryBase<string, float> { }
}
