using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class Switch : Button.Script
{
    public Sprite OffSprite, OnSprite, OnTouchSprite;

    public bool IsOn = false;

    protected override void Start()
    {
        base.Start();

        Button.RestSprite = OffSprite;
    }

    void Update()
    {
        if(IsOn && !IsDown)
        {
            if (IsPointedAt)
                Button.Image.sprite = OnTouchSprite;
            else
                Button.Image.sprite = OnSprite;
        }
    }

    protected override void OnButtonUp()
    {
        IsOn = !IsOn;
    }
}
