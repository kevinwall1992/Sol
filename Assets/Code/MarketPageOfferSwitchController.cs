using UnityEngine;
using System.Collections;

public class MarketPageOfferSwitchController : MonoBehaviour
{
    public Switch Switch;

    private void Update()
    {
        Switch.gameObject.SetActive(Switch.IsOn);
    }
}
