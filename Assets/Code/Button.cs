using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class Button : UIElement
{
    protected Sprite rest_sprite;
    protected Color rest_color;

    public Image Image;
    public Sprite TouchSprite, DownSprite;
    public Color TouchColor = Color.white, 
                 DownColor = Color.white;

    public bool IsDown { get { return IsTouched && InputUtility.IsMouseLeftPressed; } }

    protected virtual void Start()
    {
        if (Image == null)
            return;

        rest_sprite = Image.sprite;
        rest_color = Image.color;

        if (TouchSprite == null)
            TouchSprite = rest_sprite;

        if (DownSprite == null)
            DownSprite = TouchSprite;
    }

    protected virtual void Update()
    {
        if (Image != null)
        {
            if (IsDown)
            {
                Image.sprite = DownSprite;
                Image.color = DownColor;
            }
            else if (IsTouched)
            {
                Image.sprite = TouchSprite;
                Image.color = TouchColor;
            }
            else
            {
                Image.sprite = rest_sprite;
                Image.color = rest_color;
            }
        }

        if(IsTouched && InputUtility.WasMouseLeftReleased)
            OnButtonUp();
    }

    protected abstract void OnButtonUp();
}
