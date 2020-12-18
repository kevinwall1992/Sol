using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
public class WingVisualization : MonoBehaviour
{
    public Ring.Floor.Wing Wing;

    public MeshRenderer MeshRenderer;

    public RingVisualization RingController
    { get { return GetComponentInParent<RingVisualization>(); } }

    private void Update()
    {
        MeshRenderer.gameObject.SetActive(RingController.WireframeVisibility >= 1);
        if (!MeshRenderer.gameObject.activeSelf)
            return;

        Quaternion rotation;
        transform.localPosition = 
            RingController.PolarCoordinatesToPosition(Wing.Radians, 
                                                      Wing.Floor.Radius, 
                                                      out rotation);

        transform.rotation = rotation;

        MeshRenderer.transform.localScale = new Vector3(
            Wing.Width - Ring.Floor.Wing.WallThickness, 
            Wing.Floor.CeilingHeight - Ring.Floor.InterstitialSpaceThickness, 
            Wing.Floor.Ring.UnitWingDepth);
    }

    public void UpdateScale()
    {
        
    }
}