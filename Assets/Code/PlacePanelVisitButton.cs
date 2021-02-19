using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlacePanelVisitButton : Button.Script, PlacePanel.Element
{
    public List<Image> Edges;
    public TMPro.TextMeshProUGUI Text, TextShadow;

    public CanvasGroup CanvasGroup;

    public float FlashCycleLength = 1;
    public float NormalizedFlashDuration = 0.75f;

    public Color TextRestColor, 
                 TextTouchColor, 
                 TextDownColor,
                 TextErrorColor;

    void Update()
    {
        Button.Image.color = Button.Image.color.AlphaChangedTo(
            IsUserChoosingDestination() && 
            IsCraftWelcome() ? 1 : 0);

        bool is_flashing = (Time.realtimeSinceStartup %
                           FlashCycleLength /
                           FlashCycleLength <
                           NormalizedFlashDuration);

        CanvasGroup.alpha = IsUserChoosingDestination() && 
                            (IsTouched || 
                            !IsCraftWelcome() || 
                            is_flashing) ? 1 : 0;

        if (IsCraftWelcome())
            TextShadow.text = Text.text = "  Select";
        else if(this.PlacePanel().Place is NaturalSatellite)
            TextShadow.text = Text.text = "Cant Land";
        else
            TextShadow.text = Text.text = "Cant Dock";

        if (IsCraftWelcome() && IsTouched)
        {
            if (InputUtility.IsMouseLeftPressed)
                SetHighlightColor(TextDownColor);
            else
                SetHighlightColor(TextTouchColor);
        }
        else if (IsCraftWelcome())
        {
            SetHighlightColor(TextRestColor);

            if (is_flashing)
                Button.Image.sprite = Button.DownSprite;
        }
        else
            SetHighlightColor(TextErrorColor);
    }

    protected override void OnButtonUp()
    {
        if (IsUserChoosingDestination())
            The.SystemMap.TransportCraftPanel.SchedulePanel
                .AddStop(this.PlacePanel().Place);
    }

    void SetText(string text)
    {
        Text.text = text;
        TextShadow.text = text;
    }

    void SetHighlightColor(Color color)
    {
        Text.color = color;

        foreach (Image highlight_edge in Edges)
            highlight_edge.color = color;
    }

    bool IsUserChoosingDestination()
    {
        return The.SystemMap.TransportCraftPanel.SchedulePanel
            .IsUserChoosingDestination;
    }

    bool IsCraftWelcome()
    {
        if (this.PlacePanel().Place == null)
            return false;

        return this.PlacePanel().Place
            .IsWelcome(The.SystemMap.TransportCraftPanel.Craft);
    }
}
