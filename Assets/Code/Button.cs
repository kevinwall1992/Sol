using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class Button : UIElement
{
    [HideInInspector]
    public Sprite RestSprite;
    [HideInInspector]
    public Color RestColor;

    public Image Image;
    public Sprite TouchSprite, DownSprite;
    public Color TouchColor = Color.white,
                 DownColor = Color.white;

    public bool IsDown { get { return IsTouched && InputUtility.IsMouseLeftPressed; } }

    bool DontModifyImageSprite { get; set; }

    void Start()
    {
        if (Image == null)
            return;

        RestSprite = Image.sprite;
        RestColor = Image.color;

        DontModifyImageSprite = TouchSprite == null;

        if (TouchSprite == null)
            TouchSprite = RestSprite;

        if (DownSprite == null)
            DownSprite = TouchSprite;
    }

    void Update()
    {
        if (Image != null)
        {
            if (!DontModifyImageSprite)
                Image.sprite = GetDesiredSprite();

            Image.color = GetDesiredColor();
        }

        if (gameObject.HasComponent<ClickDetector>() ?
            WasClicked :
            IsTouched && InputUtility.WasMouseLeftReleased)
            OnButtonUp.Invoke();
    }

    public Color GetDesiredColor()
    {
        if (IsDown)
            return DownColor;
        else if (IsTouched)
            return TouchColor;
        else
            return RestColor;
    }

    public Sprite GetDesiredSprite()
    {
        if (IsDown)
            return DownSprite;
        else if (IsTouched)
            return TouchSprite;
        else
            return RestSprite;
    }

    public UnityEvent OnButtonUp { get; private set; } = new UnityEvent();


    [RequireComponent(typeof(Button))]
    public abstract class Script : UIElement
    {
        public Image Image;
        public Sprite TouchSprite, DownSprite;
        public Color TouchColor = Color.white,
                     DownColor = Color.white;

        public Button Button { get { return GetComponent<Button>(); } }

        public bool IsDown { get { return Button.IsDown; } }

        protected virtual void Start()
        {
            Button.OnButtonUp.AddListener(OnButtonUp);
        }

        protected abstract void OnButtonUp();
    }
}
