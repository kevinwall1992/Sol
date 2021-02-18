using UnityEngine;
using System.Collections;

public class MarketPageTransactButton : Button, MarketPage.Element
{
    bool can_transact;

    public TMPro.TextMeshProUGUI Text, ErrorText;

    public Color PurchaseColor, SellColor, ErrorColor;

    public MarketPageTransactionPanel TransactionPanel
    { get { return this.MarketPage().TransactionPanel; } }

    public Market Market { get { return this.Market(); } }

    protected override void Update()
    {
        base.Update();

        base.rest_color = TransactionPanel.IsPurchase ? PurchaseColor : SellColor;

        Text.text = "";
        ErrorText.text = "";

        can_transact = false;
        if (TransactionPanel.IsOffer)
        {
            Text.text = "Offer";
            can_transact = true;
        }
        else if (TransactionPanel.IsPurchase)
        {
            if (Scene.The.SessionUser.PrimaryBankAccount.Balance <
                TransactionPanel.TransactionCreditValue)
                ErrorText.text = "Insufficient Credits";
            else if (!TransactionPanel.Storage.CanFit(TransactionPanel.Item,
                                                     TransactionPanel.Quantity))
                ErrorText.text = "Insufficient Space";
            else if (TransactionPanel.Quantity >
                    Market.GetTotalSupply(TransactionPanel.Item.Name))
                ErrorText.text = "Insufficient Supply";
            else
            {
                Text.text = "Buy";
                can_transact = true;
            }

        }
        else
        {
            if (TransactionPanel.Quantity >
                Market.GetTotalDemand(TransactionPanel.Item.Name))
                ErrorText.text = "Insufficent Demand";
            else
            {
                Text.text = "Sell";
                can_transact = true;
            }
        }

        if (!can_transact)
            base.rest_color = ErrorColor;

        if (!IsTouched)
        {
            Text.text = (TransactionPanel.IsPurchase ? "-" : "+") +
                        TransactionPanel.TransactionCreditValue.ToShortMoneyString();
            ErrorText.text = "";
        }
    }

    protected override void OnButtonUp()
    {
        if (!can_transact)
            return;

        if (TransactionPanel.IsOffer)
        {
            if (TransactionPanel.IsPurchase)
                this.Market().PostOffer(new PurchaseOffer(
                    Scene.The.SessionUser,
                    TransactionPanel.Storage,
                    TransactionPanel.Item,
                    TransactionPanel.Quantity,
                    TransactionPanel.CreditsPerUnit));
            else
            {
                this.Market().PostOffer(new SaleOffer(
                    Scene.The.SessionUser,
                    TransactionPanel.Storage,
                    TransactionPanel.Item,
                    TransactionPanel.Quantity,
                    TransactionPanel.CreditsPerUnit));
            }
        }
        else
        {
            if (TransactionPanel.IsPurchase)
                this.Market().Purchase(
                    Scene.The.SessionUser,
                    TransactionPanel.Storage,
                    TransactionPanel.Item.Name,
                    TransactionPanel.Quantity);
            else
            {
                this.Market().Sell(
                    Scene.The.SessionUser, 
                    TransactionPanel.Storage, 
                    TransactionPanel.Item.Name, 
                    TransactionPanel.Quantity);
            }
        }
    }
}
