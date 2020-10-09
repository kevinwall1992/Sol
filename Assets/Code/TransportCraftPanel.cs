using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;


[ExecuteAlways]
[RequireComponent(typeof(EasePositionController))]
public class TransportCraftPanel : UIElement
{
    public Craft Craft;

    public enum MenuState { Hidden, Main, Schedule, Trip }
    public MenuState State = MenuState.Hidden;

    public RectTransform Corner;

    public TMPro.TextMeshProUGUI NameText;

    public ProgressBar ConditionBar;
    public FuelBar FuelBar;
    public ProgressBar StorageBar;

    public SchedulePanel SchedulePanel;
    public TripPanel TripPanel;

    public EasePositionController EasePositionController
    { get { return GetComponent<EasePositionController>(); } }

    private void Start()
    {
        transform.localPosition = GetTargetPosition();
    }

    private void Update()
    {
        NameText.text = Craft.Name;

        ConditionBar.MaximumValue = Craft.Hull.Strength;
        ConditionBar.Value = Craft.Hull.Strength - Craft.Hull.Damage;

        if (FuelBar.State == FuelBar.MeasurementState.Tonnes)
        {
            FuelBar.MaximumValue = Craft.Engine.MaximumPropellentMass / 1000;
            FuelBar.Value = Craft.Engine.PropellentMass / 1000;
        }
        else
        {
            FuelBar.MaximumValue = Craft.Engine.GetMaximumVelocityChange() / 1000;
            FuelBar.Value = Craft.Engine.GetVelocityChangeAvailable() / 1000;
        }

        StorageBar.MaximumValue = Craft.Cargo.ItemContainers.Sum(container => container.Volume);
        StorageBar.Value = Craft.Cargo.ItemContainers.Sum(container => container.ItemVolume);

        EasePositionController.TargetPosition = GetTargetPosition();
    }

    Vector3 GetTargetPosition()
    {
        switch (State)
        {
            case MenuState.Hidden:
                return Corner.localPosition.XChangedTo(-RectTransform.rect.width * 1.7f);

            case MenuState.Schedule:
            case MenuState.Trip:
                return Corner.localPosition.YChangedTo(
                    Corner.localPosition.y + RectTransform.rect.height * (State - MenuState.Main));

            default:
                return Corner.localPosition;
        }
    }


    public interface Element { }
}

public static class TransportCraftPanelElementExtensions
{
    public static TransportCraftPanel TransportCraftPanel(this TransportCraftPanel.Element element)
    { return (element as MonoBehaviour).GetComponentInParent<TransportCraftPanel>(); }
}

