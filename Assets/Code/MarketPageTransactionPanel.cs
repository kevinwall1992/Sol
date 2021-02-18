using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class MarketPageTransactionPanel : MonoBehaviour, MarketPage.Element
{
    Item item;
    public Item Item
    {
        get
        {
            if (item == null)
                item = this.MarketPage().ItemPanel.Items.First();

            return item;
        }

        set { item = value; }
    }

    public float Quantity,
                 CreditsPerUnit;

    public float InputScale = 1;


    public MarketPageBuySellSwitch BuySellSwitch;
    public MarketPageOfferSwitchController OfferSwitch;

    public MarketPageSelectStorageButton SelectStorageButton;

    public PercentageGizmo PercentageGizmo;

    public NumericTextInput QuantityInput, 
                            CreditsPerUnitInput;

    public MarketPageUnitsButton UnitsButton;

    public Image ItemImage;

    public TMPro.TextMeshProUGUI ItemNameText,
                                 AdvertisedCreditsPerUnitText, 
                                 UnitsText, 
                                 PerUnitText;

    public Storage Storage { get { return SelectStorageButton.Storage; } }

    public bool IsOffer
    {
        get { return OfferSwitch.Switch.IsOn; }
        set { OfferSwitch.Switch.IsOn = value; }
    }

    public bool IsPurchase { get { return BuySellSwitch.IsOn; } }

    public Market Market { get { return this.MarketPage().Market; } }


    public float TransactionCreditValue
    {
        get
        {
            if (IsOffer)
                return CreditsPerUnit * Quantity;
            else if (IsPurchase)
                return Market.GetPurchaseCost(Item.Name, Quantity);
            else
                return Market.GetSaleValue(Item.Name, Quantity);
        }
    }

    float MaximumQuantity
    {
        get
        {
            if (IsPurchase)
                return Market.GetTotalSupply(Item.Name);
            else
                return SelectStorageButton.Storage.GetQuantity(Item.Name);
        }
    }

    private void Start()
    {
        QuantityInput.InputField.onSubmit
            .AddListener(delegate
            {
                if (QuantityInput.InputField.wasCanceled)
                    return;

                Quantity = QuantityInput.Value * InputScale;
            });


        CreditsPerUnitInput.InputField.onValueChanged
            .AddListener(delegate
            {
                if (CreditsPerUnitInput.IsSelected)
                    IsOffer = true;
            });

        CreditsPerUnitInput.InputField.onSubmit
            .AddListener(delegate
            {
                if (CreditsPerUnitInput.InputField.wasCanceled)
                    return;

                CreditsPerUnit = CreditsPerUnitInput.Value / InputScale;
            });
    }

    private void Update()
    {
        //Item infomation

        ItemNameText.text = Item.Name;
        ItemImage.sprite = Item.ProfilePicture;

        float scaled_cost_per_unit =
            InputScale *
            Market.GetPurchaseCost(Item.Name, 0.001f) /
            0.001f;
        AdvertisedCreditsPerUnitText.text =
            scaled_cost_per_unit.ToShortMoneyString();


        //Units

        string units_string = "";
        switch (InputScale)
        {
            case 1: break;
            case 1000: units_string = "k"; break;

            default:
                units_string = InputScale.ToShortString() + " ";
                break;
        }
        units_string += Item.Units;

        if (Item.Units == "kg" && InputScale == 1000)
            units_string = "t";

        UnitsButton.Text.text = "per " + units_string;
        UnitsText.text = units_string;
        PerUnitText.text = "/" + units_string;


        //FractionUIElement

        PercentageGizmo.gameObject.SetActive(
            PercentageGizmo.IsBeingPulled ||
            QuantityInput.IsTouched());

        if (PercentageGizmo.IsBeingPulled)
        {
            Quantity = PercentageGizmo.Value * MaximumQuantity;

            if (float.IsNaN(Quantity))
                Quantity = 0;
        }
        else
            PercentageGizmo.Value = Quantity / MaximumQuantity;


        //Numeric inputs

        Quantity = Mathf.Clamp(Quantity, 0, MaximumQuantity);
        if(!QuantityInput.IsSelected)
            QuantityInput.Value = Quantity / InputScale;

        if (!IsOffer)
        {
            CreditsPerUnit = TransactionCreditValue / Quantity;

            if (float.IsNaN(CreditsPerUnit))
                CreditsPerUnit = 0;
        }

        if (!CreditsPerUnitInput.IsSelected)
            CreditsPerUnitInput.Value = CreditsPerUnit * InputScale;
    }

    public bool SufficientQuantityAvailable
    { get { return Quantity <= MaximumQuantity; } }
}
