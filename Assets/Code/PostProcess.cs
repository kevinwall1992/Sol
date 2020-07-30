using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PostProcess : MonoBehaviour
{
    RenderTexture scanline_blurred, temporary_scanline_blurred, 
                  temporary_diffused, 
                  temporary_ambient;

    public RenderTexture Input, Diverged, Pixelized, Diffused, Ambient, Output;

    public Material ScanlineBlur, 
                    Diverge,
                    Pixelize, 
                    VerticalDiffusion, 
                    HorizontalDiffusion, 
                    VerticalAmbience, 
                    HorizontalAmbience,
                    Synthesize;

    private void OnPostRender()
    {
        ValidateRenderTextures();

        System.Action<RenderTexture, RenderTexture, RenderTexture, Material, Material> Blur = 
            delegate (RenderTexture input, 
                      RenderTexture output, 
                      RenderTexture temporary, 
                      Material horizontal_blur, 
                      Material vertical_blur)
        {
            if (horizontal_blur == null || vertical_blur == null)
            {
                Material blur = horizontal_blur != null ? horizontal_blur : vertical_blur;

                Graphics.Blit(input, output, blur);
                Graphics.Blit(output, temporary, blur);
                Graphics.Blit(temporary, output, blur);
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    Graphics.Blit(i == 0 ? input : output, temporary, horizontal_blur);
                    Graphics.Blit(temporary, output, vertical_blur);
                }
            }
        };

        Blur(Input, scanline_blurred, temporary_scanline_blurred, ScanlineBlur, null);

        Graphics.Blit(scanline_blurred, Diverged, Diverge);

        Graphics.Blit(Diverged, Pixelized, Pixelize);

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

    void ValidateRenderTextures()
    {
        if (scanline_blurred != null && 
            temporary_scanline_blurred != null && 
            temporary_diffused != null && 
            temporary_ambient != null)
            return;

        System.Func<RenderTexture, RenderTexture> CopyRenderTexture = render_texture => 
            new RenderTexture(render_texture.width, render_texture.height, render_texture.depth);

        scanline_blurred = CopyRenderTexture(Input);
        temporary_scanline_blurred = CopyRenderTexture(Input);
        temporary_diffused = CopyRenderTexture(Diffused);
        temporary_ambient = CopyRenderTexture(Ambient);
    }
}
