using UnityEngine;
using System.Collections;

[ExecuteAlways]
[RequireComponent(typeof(EasePositionController))]
public class PlacePanel : UIElement
{
    public Visitable Place;

    public RectTransform Corner;

    public enum MenuState { Hidden, Main }
    public MenuState State = MenuState.Hidden;

    public TMPro.TextMeshProUGUI NameText, DescriptionText;

    public EasePositionController EasePositionController
    { get { return GetComponent<EasePositionController>(); } }

    private void Start()
    {
        transform.localPosition = GetTargetPosition();
    }

    private void Update()
    {
        if (Place != null)
        {
            NameText.text = Place.PlaceName;
            DescriptionText.text = Place.PlaceDescription;
        }

        EasePositionController.TargetPosition = GetTargetPosition();
    }

    Vector3 GetTargetPosition()
    {
        switch (State)
        {
            case MenuState.Hidden:
                return Corner.localPosition.XChangedTo(
                    Corner.localPosition.x +
                    RectTransform.rect.width * 1.7f);

            default:
                return Corner.localPosition;
        }
    }


    public interface Element { }
}

public static class PlacePanelElementExtensions
{
    public static PlacePanel PlacePanel(this PlacePanel.Element element)
    { return (element as MonoBehaviour).GetComponentInParent<PlacePanel>(); }
}

