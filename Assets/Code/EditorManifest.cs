using UnityEngine;
using System.Collections;
using RotaryHeart.Lib.SerializableDictionary;
using System.Linq;
using System.Collections.Generic;


[System.Serializable]
public class EditorManifest : SerializableDictionaryBase<string, float>
{
    public IEnumerable<Item> Samples
    {
        get
        {
            return Keys.Select(item_name => 
                The.ItemDatabase.GetSample(item_name));
        }
    }

    public float this[Item item]
    {
        get { return this[item.Name]; }
        set { this[item.Name] = value; }
    }

    public Manifest ToManifest()
    {
        return new Manifest(Samples.ToDictionary(
            sample => sample,
            sample => this[sample.Name]));
    }
}