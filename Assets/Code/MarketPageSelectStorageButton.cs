using UnityEngine;
using System.Collections;
using System.Linq;

public class MarketPageSelectStorageButton : Button, MarketPage.Element
{
    Craft storage_craft;

    bool UseWarehouse
    { get { return storage_craft == null; } }

    public Storage Storage
    {
        get
        {
            if (UseWarehouse)
                return this.Market().Station
                   .GetStorage(Scene.The.SessionUser);
            else
                return storage_craft.Cargo;
        }
    }

    public TMPro.TextMeshProUGUI Text;

    protected override void Update()
    {
        base.Update();

        if (UseWarehouse)
            Text.text = "Warehouse";
        else
            Text.text = storage_craft.Name;
    }

    protected override void OnButtonUp()
    {
        if (UseWarehouse)
            storage_craft = Scene.The.SessionUser.Crafts.First();
        else if (storage_craft == Scene.The.SessionUser.Crafts.Last())
            storage_craft = null;
        else
            storage_craft = Scene.The.SessionUser.Crafts.NextElement(storage_craft);
    }
}
