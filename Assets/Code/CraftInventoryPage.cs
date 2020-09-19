using UnityEngine;
using System.Collections;

[RequireComponent(typeof(InventoryPage))]
public class CraftInventoryPage : MonoBehaviour
{
    public Craft Craft;

    public InventoryPage InventoryPage { get { return GetComponent<InventoryPage>(); } }

    private void Start()
    {
        InventoryPage.ItemContainers = Craft.Storage;
    }

    private void Update()
    {
        InventoryPage.AddressLine1.text = Craft.Primary.Name;
        InventoryPage.AddressLine2.text = Craft.Name;
    }
}
