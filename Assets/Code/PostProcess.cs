using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PostProcess : MonoBehaviour
{
    public RenderTexture Input, Intermediate0, Intermediate1, Output;

    public Material Pixelized;

    private void OnPostRender()
    {
        Graphics.Blit(Input, Output, Pixelized);
    }
}
