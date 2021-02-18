using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MarketPage : Page
{
    public Market Market;

    public ItemCollectionPanel ItemPanel;

    public TMPro.TMP_InputField SearchBar;

    public MarketPageTransactionPanel TransactionPanel;

    public TMPro.TextMeshProUGUI MarketNameText, StationNameText;

    private void Start()
    {
        ItemPanel.Items = Market.Wares;
    }

    private void Update()
    {
        MarketNameText.text = "Wholesale Market";
        StationNameText.text = Market.Station.PlaceName;

        if (ItemPanel.SelectedItem == null)
            ItemPanel.SelectedItem = ItemPanel.Elements.First().Item;

        TransactionPanel.Item = ItemPanel.SelectedItem;
    }


    public interface Element { }
}

public static class MarketPageElementExtensions
{
    public static MarketPage MarketPage(this MarketPage.Element element)
    { return (element as MonoBehaviour).GetComponentInParent<MarketPage>(); }

    public static Market Market(this MarketPage.Element element)
    { return element.MarketPage().Market; }
}