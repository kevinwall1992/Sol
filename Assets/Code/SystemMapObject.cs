using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Linq;

[ExecuteAlways]
public class SystemMapObject : MonoBehaviour
{
    public Image Image;
    public Sprite Icon;

    public CanvasGroup ImageCanvasGroup;

    public LineController LineController;
    public TravelingElement OrbitLine;

    public float SizeChangeSpeed = 3;
    public float AppearSpeed = 3;

    public float VisualSize;

    [HideInInspector]
    public SystemMapObject Parent;

    public bool IsFocused
    { get { return The.SystemMap.FocusedObject == this; } }

    public SystemMap SystemMap { get { return GetComponentInParent<SystemMap>(); } }

    private void Start()
    {
        OrbitLine.Destination = The._3DUIElementsContainer;

        
    }

    private void Update()
    {
        Image.sprite = Icon;

        //Necessary to prevent culling bug in Unity...
        Color color = Image.color;
        Image.color = Color.black;
        Image.color = color;

        //Size
        RectTransform image_transform = Image.transform as RectTransform;
        image_transform.sizeDelta = image_transform.sizeDelta.Lerped(
            VisualSize * Vector2.one, SizeChangeSpeed * Time.deltaTime);


        //Orbit line and line occlusion
        LineController.Line.enabled = !IsFocused &&
                                      Image.IsTouched();

        MaterialPropertyBlock material_property_block = new MaterialPropertyBlock();
        LineController.Line.GetPropertyBlock(material_property_block);

        material_property_block.SetFloat("PathLength", LineController.Length);
        material_property_block.SetVector("ObjectPosition",
            The.Canvas.transform.InverseTransformPoint(transform.position));
        material_property_block.SetFloat("ObjectSize", VisualSize);

        LineController.Line.SetPropertyBlock(material_property_block);


        //Visibility
        if (Application.isPlaying)
        {
            float target_alpha = 0;

            if (this == SystemMap.Root ||
                SystemMap.FocusedObject.IsChildOf(Parent) ||
                Parent.IsFocused)
                target_alpha = 1;

            ImageCanvasGroup.blocksRaycasts = target_alpha > 0;

            ImageCanvasGroup.alpha =
                Mathf.Lerp(ImageCanvasGroup.alpha,
                           target_alpha,
                           AppearSpeed * Time.deltaTime);
        }
    }
}
