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
            !UnityEditor.EditorApplication.isPlaying &&
            !HideTools);

        Wrapper.transform.SetParent(
            UnityEditor.EditorApplication.isPlaying ?
            transform :
            Preview,
            false);

        if(AutomaticPreviewSize)
            Preview.sizeDelta =
                Scene.The.Style.MonitorResolution -
                new Vector2Int(HalfWindowPreview ? Scene.The.Style.MonitorResolution.x / 2 : 0, 0) -
                new Vector2Int(2, 1) * Scene.The.Style.StandardEdgeThickness -
                new Vector2Int(0, Scene.The.Prefabs.Window.DefaultTitleBarHeight) -
                new Vector2Int(0, Scene.The.Taskbar.Height);

        PreviewBackground.color = 
            UnityEditor.EditorApplication.isPlaying ? 
            Color.clear : 
            Scene.The.Prefabs.Window.DefaultBackgroundColor;
    }
}
