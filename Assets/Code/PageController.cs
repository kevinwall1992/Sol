using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteAlways]
public class PageController : MonoBehaviour
{
    public RectTransform Wrapper, Tools, Preview;

    public bool HideTools;
    public bool AutomaticPreviewSize = true;
    public bool HalfWindowPreview = true;

    public Image PreviewBackground { get { return Preview.GetComponent<Image>(); } }

    void Update()
    {
        Tools.gameObject.SetActive(
            !Application.isPlaying &&
            !HideTools);

        Wrapper.transform.SetParent(
            Application.isPlaying ?
            transform :
            Preview,
            false);

        if(AutomaticPreviewSize)
            Preview.sizeDelta =
                The.Style.MonitorResolution -
                new Vector2Int(HalfWindowPreview ? The.Style.MonitorResolution.x / 2 : 0, 0) -
                new Vector2Int(2, 1) * The.Style.StandardEdgeThickness -
                new Vector2Int(0, The.Prefabs.Window.DefaultTitleBarHeight) -
                new Vector2Int(0, The.Taskbar.Height);

        PreviewBackground.color = 
            Application.isPlaying ? 
            Color.clear : 
            The.Prefabs.Window.DefaultBackgroundColor;
    }
}
