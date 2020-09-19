using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class Switch : Button
{
    public Sprite OffSprite, OnSprite, OnTouchSprite;

    public bool IsOn = false;

    protected override void Start()
    {
        base.Start();

        rest_sprite = OffSprite;
    }

    protected override void Update()
    {
        base.Update();

        if(IsOn && !IsDown)
        {
            if (IsPointedAt)
                Image.sprite = OnTouchSprite;
            else
                Image.sprite = OnSprite;
        }
    }

    protected override void OnButtonUp()
    {
        IsOn = !IsOn;
    }
}
