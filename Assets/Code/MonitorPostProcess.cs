using UnityEngine;
using System.Collections;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class MonitorPostProcess : MonoBehaviour
{
    public RenderTexture Input, Output;

    public Material AntiMoire;

    void Start()
    {
    }

    private void OnPostRender()
    {
        Graphics.Blit(Input, Output, AntiMoire);
    }
}