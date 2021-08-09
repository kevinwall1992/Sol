using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InventoryPage : Page
{
    public ItemCollectionPanel ItemPanel;

    public TMPro.TextMeshProUGUI AddressLine1, AddressLine2;

    public Inventory Inventory;


    private void Start()
    {
        
    }

    private void Update()
    {
        if (ItemPanel.Items == null)
            ItemPanel.Items = Inventory.Manifest.Samples;

        if (ItemPanel.SelectedItem != null)
        {
            The.ItemPage.Item = ItemPanel.SelectedItem;
            The.ItemPage.Window.Open();
            ItemPanel.SelectedItem = null;
        }
    }
}
