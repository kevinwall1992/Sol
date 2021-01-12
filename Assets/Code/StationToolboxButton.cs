using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class StationToolboxButton : Button
{
    public RingVisualizationOverlay Overlay;

    public Image SelectionFrame;

    public bool IsSelected { get { return StationToolbox.SelectedButton == this; } }

    public StationToolbox StationToolbox
    { get { return GetComponentInParent<StationToolbox>(); } }

    public Interactor Interactor_
    { get { return GetComponentInChildren<Interactor>(); } }

    protected override void Update()
    {
        base.Update();

        if (Interactor_ != null &&
            IsSelected &&
            Scene.The.StationViewer.IsTouched)
            Overlay.gameObject.SetActive(Interactor_.OnInteract());
        else
            Overlay.gameObject.SetActive(false);

        SelectionFrame.gameObject.SetActive(IsSelected);

    }

    protected override void OnButtonUp()
    {
        if (IsTouched)
        {
            if (StationToolbox.SelectedButton == this)
                StationToolbox.SelectedButton = null;
            else
                StationToolbox.SelectedButton = this;
        }
    }

    [RequireComponent(typeof(StationToolboxButton))]
    public abstract class Interactor : MonoBehaviour
    {
        public StationToolboxButton StationToolboxButton
        { get { return GetComponent<StationToolboxButton>(); } }

        public StationViewer StationViewer
        { get { return Scene.The.StationViewer; } }

        public RingVisualization RingVisualization
        { get { return StationViewer.StationVisualization.SelectedRing; } }

        public Ring Ring
        { get { return RingVisualization.Ring; } }

        public RingVisualizationOverlay Overlay
        { get { return StationToolboxButton.Overlay; } }

        public Vector2 PolarCoordinates
        {
            get
            {
                Ray ray = StationViewer.GetRayFromCursorPosition();

                return RingVisualization.PolarCoordinatesFromRay(ray);
            }
        }

        public float Radians { get { return PolarCoordinates.x; } }
        public float Radius { get { return PolarCoordinates.y; } }

        public abstract bool OnInteract();
    }
}
