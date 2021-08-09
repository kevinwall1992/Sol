using UnityEngine;
using System.Collections;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using System.Linq;

public class Machine : Item.Script
{
    public Recipe Recipe;
    public float WorkPerSecond;

    public bool IsOn = false;

    public float CyclesPerSecond
    { get { return WorkPerSecond / Recipe.Work; } }

    public float CyclesPerDay
    { get { return CyclesPerSecond * 60 * 60 * 24; } }

    public float QuantityPerSecond
    { get { return Recipe.OutputQuantity * CyclesPerSecond; } }

    public Inventory Inventory
    { get { return Item.Inventory; } }

    private void Update()
    {
        if (!IsOn || Inventory.GetSpaceAvailable(Recipe.Output) == 0)
            return;

        float cycles = Item.Quantity *
                       CyclesPerSecond *
                       The.Clock.DeltaTime;

        cycles = Mathf.Min(cycles, 
            Recipe.Inputs.Samples.Min(sample => Inventory.GetQuantity(sample) /
                                                Recipe.Inputs[sample]));

        foreach (Item sample in Recipe.Inputs.Samples)
        {
            float quantity = Recipe.Inputs[sample] * cycles;

            Inventory.TakeOut(sample, quantity);
        }

        Inventory.PutIn(Recipe.Output, cycles * Recipe.OutputQuantity);
    }
}
