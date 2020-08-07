using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PostProcess : MonoBehaviour
{
    RenderTexture scanline_blurred, temporary_scanline_blurred,
                  response0, response1,
                  temporary_diffused, 
                  temporary_ambient;

    bool is_even_frame = true;

    public RenderTexture Input, Diverged, Pixelized, Diffused, Ambient, Output;

    public Material ScanlineBlur, 
                    Diverge,
                    Respond,
                    Pixelize, 
                    VerticalDiffusion, 
                    HorizontalDiffusion, 
                    VerticalAmbience, 
                    HorizontalAmbience,
                    Synthesize;

    public bool UseScanlineBlur;

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

        if (UseScanlineBlur)
        {
            Blur(Input, scanline_blurred, temporary_scanline_blurred, ScanlineBlur, null);
            Graphics.Blit(scanline_blurred, Diverged, Diverge);
        }
        else
            Graphics.Blit(Input, Diverged, Diverge);

        if (UnityEditor.EditorApplication.isPlaying)
        {
            RenderTexture previous_response = response1;
            RenderTexture response = response0;
            if (!is_even_frame)
            {
                previous_response = response0;
                response = response1;
            }
            is_even_frame = !is_even_frame;

            Respond.SetFloat("time_delta", Time.deltaTime);
            Graphics.Blit(previous_response, response, Respond);

            Graphics.Blit(response, Pixelized, Pixelize);
        }
        else
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

        Synthesize.SetFloat("relative_image_size", 
                            Pixelize.GetFloat("RelativeImageSize"));
        Graphics.Blit(null, Output, Synthesize);
    }

    void ValidateRenderTextures()
    {
        if (scanline_blurred != null && 
            temporary_scanline_blurred != null && 
            response0 != null && 
            response1 != null &&
            temporary_diffused != null && 
            temporary_ambient != null)
            return;

        System.Func<RenderTexture, RenderTexture> CreateRenderTexture = render_texture => 
            new RenderTexture(render_texture.width, render_texture.height, render_texture.depth);

        scanline_blurred = CreateRenderTexture(Input);
        temporary_scanline_blurred = CreateRenderTexture(Input);
        response0 = CreateRenderTexture(Input);
        response1 = CreateRenderTexture(Input);
        temporary_diffused = CreateRenderTexture(Diffused);
        temporary_ambient = CreateRenderTexture(Ambient);
    }
}
