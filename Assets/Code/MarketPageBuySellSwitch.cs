using UnityEngine;
using System.Collections;

public class MarketPageBuySellSwitch : MonoBehaviour
{
    public Switch Switch;
    public TMPro.TextMeshProUGUI Text;

    public bool IsOn
    {
        get { return Switch.IsOn; }
        set { Switch.IsOn = value; }
    }

    private void Update()
    {
        if (IsOn)
            Text.text = "Buying";
        else
            Text.text = "Selling";
    }
}
