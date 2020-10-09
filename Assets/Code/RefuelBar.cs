using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(ProgressBar))]
public class RefuelBar : UIElement
{
    public RefuelAmountButton FirstTransferButton, 
                              SecondTransferButton, 
                              FillerupButton;

    public RectTransform PurchaseBar;

    public float TargetFuelMass;

    public float PurchaseMass
    { get { return Mathf.Max(0, TargetFuelMass - ProgressBar.Value); } }

    public ProgressBar ProgressBar
    { get { return GetComponent<ProgressBar>(); } }

    public RefuelingPage RefuelPage
    { get { return GetComponentInParent<RefuelingPage>(); } }

    private void Update()
    {
        Craft craft = RefuelPage.Craft;

        ProgressBar.MaximumValue = craft.Engine.MaximumPropellentMass;
        ProgressBar.Value = craft.Engine.PropellentMass;

        FillerupButton.RequiredFuelMass = ProgressBar.MaximumValue;


        //Size and position the fuel requirement indicators. 
        bool show_first_transfer_button = false;
        bool show_second_transfer_button = false;

        if (craft.Navigation.UpcomingTransfers.Count > 0)
        {
            FirstTransferButton.gameObject.SetActive(true);

            float fuel_mass_required = GetCumulativeFuelMassRequired(
                          craft.Navigation.UpcomingTransfers[0]);

            if (fuel_mass_required <= ProgressBar.MaximumValue)
            {
                FirstTransferButton.transform.localPosition =
                FirstTransferButton.transform.localPosition.XChangedTo(
                    RectTransform.rect.width *
                    fuel_mass_required / ProgressBar.MaximumValue);

                FirstTransferButton.RequiredFuelMass = fuel_mass_required;

                show_first_transfer_button = true;


                if (RefuelPage.Craft.Navigation.UpcomingTransfers.Count > 1)
                {
                    SecondTransferButton.gameObject.SetActive(true);

                    fuel_mass_required = GetCumulativeFuelMassRequired(
                            craft.Navigation.UpcomingTransfers[0]);

                    if (fuel_mass_required >= ProgressBar.MaximumValue)
                    {
                        SecondTransferButton.transform.localPosition =
                            SecondTransferButton.transform.localPosition.XChangedTo(
                                RectTransform.rect.width *
                                fuel_mass_required / ProgressBar.MaximumValue);

                        SecondTransferButton.RequiredFuelMass = fuel_mass_required;

                        show_second_transfer_button = true;
                    }
                }
            }
        }

        FirstTransferButton.gameObject.SetActive(show_first_transfer_button);
        SecondTransferButton.gameObject.SetActive(show_second_transfer_button);


        //Size and position the bar representing fuel purchase quantity
        PurchaseBar.localPosition = ProgressBar.Bar.localPosition + 
                                    new Vector3(ProgressBar.Bar.rect.width, 0);
        PurchaseBar.sizeDelta = PurchaseBar.sizeDelta.XChangedTo(
            RectTransform.rect.width * PurchaseMass / ProgressBar.MaximumValue);


        //Set target fuel (and thus purchase quantity) based on where user clicks
        if(InputUtility.IsMouseLeftPressed && IsTouched)
        {
            TargetFuelMass = 
                ProgressBar.MaximumValue *
                RectTransform.PixelPositionToLocalPosition(
                    Scene.The.Cursor.PixelPointedAt).x / 
                RectTransform.rect.width;
        }
    }

    public float GetCumulativeFuelMassRequired(Navigation.Transfer transfer)
    {
        Craft craft = RefuelPage.Craft;

        List<Navigation.Transfer> transfers;
        if (craft.Navigation.UpcomingTransfers.Contains(transfer))
            transfers = craft.Navigation.UpcomingTransfers
                .GetRange(0, craft.Navigation.UpcomingTransfers.IndexOf(transfer) + 1);
        else
        {
            transfers = craft.Navigation.UpcomingTransfers.ToList();
            transfers.Append(transfer);
        }

        float reaction_mass = 0;

        foreach (Navigation.Transfer transfer_ in transfers)
        {
            foreach (Navigation.Transfer.Maneuver maneuver in transfer_.Maneuvers)
            {
                float dV = craft.Engine.GetVelocityChangeRequired(maneuver, craft.Motion);

                reaction_mass = dV * (craft.EmptyTankMass + reaction_mass) / 
                                craft.Engine.ExhaustVelocity;
            }
        }

        return reaction_mass;
    }
}
