using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PostProcess : MonoBehaviour
{
    RenderTexture temporary_diffused, temporary_ambient;

    public RenderTexture Input, Output, Pixelized, Diffused, Ambient;

    public Material Pixelize, 
                    VerticalDiffusion, 
                    HorizontalDiffusion, 
                    VerticalAmbience, 
                    HorizontalAmbience,
                    Synthesize;

    private void OnPostRender()
    {
        if (temporary_diffused == null)
            temporary_diffused = new RenderTexture(Diffused.width, Diffused.height, Diffused.depth);
        if (temporary_ambient == null)
            temporary_ambient = new RenderTexture(Ambient.width, Ambient.height, Ambient.depth);

        Graphics.Blit(Input, Pixelized, Pixelize);

        for (int i = 0; i < 3; i++)
        {
            Graphics.Blit(i == 0 ? Pixelized : Diffused, temporary_diffused, VerticalDiffusion);
            Graphics.Blit(temporary_diffused, Diffused, HorizontalDiffusion);
        }

        for (int i = 0; i < 3; i++)
        {
            Graphics.Blit(i == 0 ? Pixelized : Ambient, temporary_ambient, VerticalAmbience);
            Graphics.Blit(temporary_ambient, Ambient, HorizontalAmbience);
        }

        Graphics.Blit(null, Output, Synthesize);
    }
}
