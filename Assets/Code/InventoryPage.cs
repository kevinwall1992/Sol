using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InventoryPage : Page
{
    public ItemCollectionPanel ItemPanel;

    public TMPro.TextMeshProUGUI AddressLine1, AddressLine2;

    public Storage Storage;


    private void Start()
    {
        ItemPanel.Items = Storage.Items;
    }

    private void Update()
    {
        if(ItemPanel.SelectedItem != null)
        {
            The.ItemPage.Item = ItemPanel.SelectedItem;
            The.ItemPage.Window.Open();
            ItemPanel.SelectedItem = null;
        }
    }
}
