using UnityEngine;
using System.Collections;

public class RefuelingPage : Page
{
    public RefuelBar RefuelBar;
    public RefuelPayButton RefuelPayButton;

    public TMPro.TextMeshProUGUI CostPerUnitText;

    public Craft Craft;

    public float DollarsPerKilogram;

    private void Update()
    {
        CostPerUnitText.text = 
            "$" + (DollarsPerKilogram * 1000).ToString("F2");
    }
}
