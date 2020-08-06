using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class Button : UIElement
{
    Sprite rest_sprite;
    Color rest_color;

    public Image Image;
    public Sprite TouchSprite, DownSprite;
    public Color TouchColor = Color.white, 
                 DownColor = Color.white;

    protected virtual void Start()
    {
        rest_sprite = Image.sprite;
        rest_color = Image.color;

        if (TouchSprite == null)
            TouchSprite = rest_sprite;

        if (DownSprite == null)
            DownSprite = TouchSprite;
    }

    protected virtual void Update()
    {
        if (IsTouched)
        {
            if (InputUtility.IsMouseLeftPressed)
            {
                Image.sprite = DownSprite;
                Image.color = DownColor;
            }
            else
            {
                Image.sprite = TouchSprite;
                Image.color = TouchColor;
            }

            if (InputUtility.WasMouseLeftReleased)
                OnButtonUp();
        }
        else
        {
            Image.sprite = rest_sprite;
            Image.color = rest_color;
        }
    }

    protected abstract void OnButtonUp();
}
