using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InventoryPage : Page
{
    public Grid ElementGrid;
    public Switch IconSwitch;

    public TMPro.TextMeshProUGUI AddressLine1, AddressLine2;

    public IEnumerable<ItemContainer> ItemContainers;

    public int IconMargin, LineMargin;

    public IEnumerable<IconInventoryElement> IconInventoryElements
    { get { return ElementGrid.GetComponentsInChildren<IconInventoryElement>(); } }

    public IEnumerable<LineInventoryElement> LineInventoryElements
    { get { return ElementGrid.GetComponentsInChildren<LineInventoryElement>(); } }

    bool IsDisplayingIcons
    {
        get
        {
            if (IconInventoryElements.Count() > 0)
                return true;
            else if (LineInventoryElements.Count() > 0)
                return false;

            return IconSwitch.IsOn;
        }
    }

    IEnumerable<Item> UnderlyingItems
    {
        get
        {
            return ItemContainers
                .SelectMany(item_container => item_container.Items.Values);
        }
    }

    IEnumerable<Item> ItemsListed
    {
        get
        {
            if (IsDisplayingIcons)
                return IconInventoryElements.Select(element => element.Item);
            else
                return LineInventoryElements.Select(element => element.Item);
        }
    }

    private void Start()
    {
        ClearElements();
    }

    private void Update()
    {
        ValidateElements();

        if (IsDisplayingIcons)
            ElementGrid.Margin = IconMargin;
        else
            ElementGrid.Margin = LineMargin;
    }

    void ClearElements()
    {
        foreach (IconInventoryElement element in IconInventoryElements.ToList())
            GameObject.Destroy(element.gameObject);
    }

    void ValidateElements()
    {
        if (IsDisplayingIcons != IconSwitch.IsOn)
            ClearElements();

        if (IsDisplayingIcons)
        {
            foreach (IconInventoryElement element in IconInventoryElements.ToList())
                if (!UnderlyingItems.Contains(element.Item))
                    GameObject.Destroy(element.gameObject);
        }
        else
        {
            foreach (LineInventoryElement element in LineInventoryElements.ToList())
                if (!UnderlyingItems.Contains(element.Item))
                    GameObject.Destroy(element.gameObject);
        }

        foreach (Item item in UnderlyingItems)
            if (!ItemsListed.Contains(item))
            {
                GameObject element;

                if (IsDisplayingIcons)
                {
                    IconInventoryElement icon_inventory_element = 
                        GameObject.Instantiate(IconInventoryElementPrefab);
                    icon_inventory_element.Item = item;
                    element = icon_inventory_element.gameObject;
                }
                else
                {
                    LineInventoryElement line_inventory_element = 
                        GameObject.Instantiate(LineInventoryElementPrefab);
                    line_inventory_element.Item = item;
                    element = line_inventory_element.gameObject;
                }

                element.transform.SetParent(ElementGrid.transform, false);
            }
    }


    public IconInventoryElement IconInventoryElementPrefab;
    public LineInventoryElement LineInventoryElementPrefab;
}
