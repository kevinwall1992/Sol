using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
public class WingVisualization : MonoBehaviour
{
    MaterialPropertyBlock material;

    public Ring.Floor.Wing Wing;

    public MeshRenderer MeshRenderer;
    public Transform MeshContainer;

    public MeshRenderer SelectionMeshRenderer;

    public Color Color;

    public RingVisualization RingVisualization
    { get { return GetComponentInParent<RingVisualization>(); } }

    private void Update()
    {
        if(RingVisualization.WireframeVisibility < 1)
        {
            MeshRenderer.gameObject.SetActive(false);
            return;
        }
        MeshRenderer.gameObject.SetActive(true);

        Quaternion rotation;
        transform.localPosition =
            RingVisualization.PolarCoordinatesToPosition(
                Wing.RadianCenter,
                Wing.Floor.Radius,
                out rotation);
        transform.localRotation = rotation;

        MeshContainer.transform.localScale = new Vector3(
            Wing.Width - Ring.Floor.Wing.WallThickness,
            Wing.Floor.CeilingHeight - Ring.Floor.InterstitialSpaceThickness,
            Wing.Floor.Ring.UnitWingDepth);


        if (material == null)
        {
            material = new MaterialPropertyBlock();
            MeshRenderer.GetPropertyBlock(material);
        }

        material.SetColor("_Color", Color);

        MeshRenderer.SetPropertyBlock(material);
    }
}