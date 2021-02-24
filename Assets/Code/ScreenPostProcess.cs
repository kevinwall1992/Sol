using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ScreenPostProcess : MonoBehaviour
{
    RenderTexture input, 
                  diverged, 
                  scanline_blurred, temporary_scanline_blurred,
                  response0, response1,
                  temporary_diffused, 
                  temporary_ambient;

    bool is_even_frame = true;

    public Camera Camera;

    public RenderTexture Pixelized, Diffused, Ambient, Output;

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

    void Start()
    {
        input = new RenderTexture(The.Style.MonitorResolution.x, The.Style.MonitorResolution.y, 24);
        diverged = new RenderTexture(input);

        scanline_blurred = new RenderTexture(input);
        temporary_scanline_blurred = new RenderTexture(input);
        response0 = new RenderTexture(input);
        response1 = new RenderTexture(input);
        temporary_diffused = new RenderTexture(Diffused);
        temporary_ambient = new RenderTexture(Ambient);

        Camera.targetTexture = input;
    }

    private void OnPostRender()
    {
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


        //Poor signal, then
        //Poor color convergence
        if (UseScanlineBlur)
        {
            Blur(input, scanline_blurred, temporary_scanline_blurred, ScanlineBlur, null);
            Graphics.Blit(scanline_blurred, diverged, Diverge);
        }
        else
            Graphics.Blit(input, diverged, Diverge);


        //Slow pixels
        Pixelize.SetInt("MonitorResolutionX", The.Style.MonitorResolution.x);
        Pixelize.SetInt("MonitorResolutionY", The.Style.MonitorResolution.y);
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
            Respond.SetTexture("Signal", diverged);
            Graphics.Blit(previous_response, response, Respond);

            Graphics.Blit(response, Pixelized, Pixelize);
        }
        else
            Graphics.Blit(diverged, Pixelized, Pixelize);


        //Near scattering
        for (int i = 0; i < 3; i++)
        {
            Graphics.Blit(i == 0 ? Pixelized : Diffused, temporary_diffused, VerticalDiffusion);
            Graphics.Blit(temporary_diffused, Diffused, HorizontalDiffusion);
        }

        //Far Scattering
        for (int i = 0; i < 3; i++)
        {
            Graphics.Blit(i == 0 ? Pixelized : Ambient, temporary_ambient, VerticalAmbience);
            Graphics.Blit(temporary_ambient, Ambient, HorizontalAmbience);
        }


        //Add it all together
        //(Also handles effect of light reflecting off screen substrate)
        Synthesize.SetInt("MonitorResolutionX", The.Style.MonitorResolution.x);
        Synthesize.SetInt("MonitorResolutionY", The.Style.MonitorResolution.y);
        Synthesize.SetFloat("relative_image_size", 
                            Pixelize.GetFloat("RelativeImageSize"));

        Graphics.Blit(null, Output, Synthesize);
    }
}
