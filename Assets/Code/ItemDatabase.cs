using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

//(1)
//When reasoning about Items where some instances may be equivalent for your
//purposes, such as when using a Market to find rocket fuel, you may imagine 
//coming up with a search method that determines which items available fit your
//requirements. If you already have access to an Item that fits your 
//requirements, then you can compare it to other items by Item.IsEquivalent(). 
//If this method returns true, then the other item is guaranteed (guarantee is 
//contractual, not inherent) to be equivalent for all purposes (Note however, 
//that the opposite is not necessarily true; if two items are not 
//.IsEquivalent(), that does not mean they aren't equivalent under some or all 
//metrics). Thus, an item is itself a search term to find more items that meet
//your requirements. 

//(2)
//Because .IsEquivalent() == true implies functional equality, structures that
//contain Items are allowed to merge equivalent Items. 

//Because of (1), in order to use the market and other structures containing 
//Items, examples are expected in order to search for equivalents. However, 
//because of (2), an example of a fungible item has a unknown lifetime. 
//Therefore, examples that are stored need to have lifetime guarantees. Thats 
//where ItemDatabase comes in. It makes copies of fungible Items and stores 
//them through the entire run of the application, ensuring references to them 
//remain valid. 

//For Items that are not fungible (known as "unique"), it merely stores a 
//reference (unique Items may change over their lifetime; therefore a copy 
//cannot be guaranteed to be equivalent to the original). 
//Because "samples" of unique Items are references, its is preferable that the
//lifetime of unique items be longer than the samples of that item, to prevent
//reading a deleted object. There are three options here: destruction is not
//possible while samples exist (f.e. Crafts can only be destroyed after leaving
//port, can't leave port until all offers for Craft are deleted), samples are
//deleted on destruction (f.e. Craft.Destroy() notifies markets so they can
//delete offers), or permanent lifetime (f.e. store dead Crafts in a 
//"graveyard" forever). 

//Definition of terms
//"Example": In the context of Items, an example is a Item who's intended use 
//    is to search for equivalent items, or to be used to reason about such 
//    items (such as querying Item.GetVolumePerUnit())
//"Sample": An example with a lifetime. These examples should be safe to store.
//    Samples of fungible items are stored in ItemDatabase, and are guaranteed 
//    to not be destroyed. Unique items lifetimes are not guaranteed by 
//    ItemDatabase. 

public class ItemDatabase : MonoBehaviour
{
    ItemSet samples = new ItemSet();

    public IEnumerable<Item> Samples { get { return samples.Elements; } }

    public Item GetSample(Item item)
    {
        if (!samples.Contains(item))
        {
            Item sample;

            if (item.IsFungible())
            {
                sample = item.Copy();
                sample.Quantity = 1;

                sample.transform.SetParent(transform);
            }
            else
                sample = item;

            samples.Add(sample);
        }

        return samples.Get(item);
    }

    public IEnumerable<Item> GetSamples(IEnumerable<Item> commodities)
    {
        return commodities
            .Select(item => item.TakeSample())
            .Distinct();
    }
}

public static class ItemDatabaseExtensions
{
    public static Item TakeSample(this Item item)
    {
        return The.ItemDatabase.GetSample(item);
    }

    public static IEnumerable<Item> TakeSamples(this IEnumerable<Item> commodities)
    {
        return The.ItemDatabase.GetSamples(commodities);
    }

    public static Item GetSample(this ItemDatabase item_database, string name)
    {
        return item_database.Samples
            .FirstOrDefault(sample => sample.Name == name);
    }
}
