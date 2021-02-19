using UnityEngine;
using System.Collections;

public class MarketPageUnitsButton : Button.Script, MarketPage.Element
{
    public Label Label;

    public TMPro.TextMeshProUGUI Text;

    protected void Update()
    {
        Label.BackgroundColor = Button.Image.color;
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
