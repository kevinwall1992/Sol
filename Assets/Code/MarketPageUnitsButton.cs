using UnityEngine;
using System.Collections;

public class MarketPageUnitsButton : Button, MarketPage.Element
{
    public Label Label;

    public TMPro.TextMeshProUGUI Text;

    protected override void Update()
    {
        base.Update();

        Label.BackgroundColor = base.Image.color;
    }

    protected override void OnButtonUp()
    {
        MarketPageTransactionPanel transaction_panel = 
            this.MarketPage().TransactionPanel;

        if (transaction_panel.InputScale == 1)
            transaction_panel.InputScale = 1000;
        else
            transaction_panel.InputScale = 1;
    }
}
