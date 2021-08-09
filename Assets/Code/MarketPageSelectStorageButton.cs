using UnityEngine;
using System.Collections;
using System.Linq;

public class MarketPageSelectStorageButton : Button.Script, MarketPage.Element
{
    Craft storage_craft;

    bool UseWarehouse
    { get { return storage_craft == null; } }

    public Inventory Inventory
    {
        get
        {
            if (UseWarehouse)
                return this.Market().Station.Craft.GetInventory(The.SessionUser);
            else
                return storage_craft.Cargo;
        }
    }

    public TMPro.TextMeshProUGUI Text;

    void Update()
    {
        if (UseWarehouse)
            Text.text = "Warehouse";
        else
            Text.text = storage_craft.Name;
    }

    protected override void OnButtonUp()
    {
        if (UseWarehouse)
            storage_craft = The.SessionUser.Crafts.First();
        else if (storage_craft == The.SessionUser.Crafts.Last())
            storage_craft = null;
        else
            storage_craft = The.SessionUser.Crafts.NextElement(storage_craft);
    }
}
