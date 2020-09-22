using UnityEngine;
using System.Collections;

public class RefuelPayButton : Button
{
    public TMPro.TextMeshProUGUI Price;

    public RefuelingPage RefuelPage
    { get { return GetComponentInParent<RefuelingPage>(); } }

    protected override void OnButtonUp()
    {
        throw new System.NotImplementedException();
    }

    protected override void Update()
    {
        base.Update();

        if (IsTouched)
            Price.text = "Pay?";
        else
            Price.text = "$" + ((int)(
                RefuelPage.DollarsPerKilogram * 
                RefuelPage.RefuelBar.PurchaseMass)).ToString();
    }
}
