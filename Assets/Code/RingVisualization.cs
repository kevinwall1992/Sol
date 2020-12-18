using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RingVisualization : MonoBehaviour
{
    float outer_radius_displayed = 0;
    List<int> wings_displayed_per_floor = new List<int>();

    public Ring Ring;

    public MeshRenderer MeshRenderer;

    public float Linearity;
    public float WingVisibility;
    public float WireframeVisibility;

    public int FloorResolution = 256;
    public int MaxFloorCount = 128;
    public int MaxWingCountPerFloor = 1024;

    public IEnumerable<WingVisualization> WingVisualizations
    { get { return GetComponentsInChildren<WingVisualization>(); } }

    void Update()
    {
        MaterialPropertyBlock material = new MaterialPropertyBlock();
        MeshRenderer.GetPropertyBlock(material);

        bool regenerate = false;
        if (outer_radius_displayed != Ring.GroundFloorRadius)
            regenerate = true;

        if (wings_displayed_per_floor.Count != Ring.Floors.Count)
            regenerate = true;
        else
            for (int i = 0; i < Ring.Floors.Count; i++)
                if (Ring.Floors[i].Wings.Count != wings_displayed_per_floor[i])
                    regenerate = true;

        if (regenerate)
        {
            //Floor data buffers

            float[] floor_indices = new float[FloorResolution];

            float[] floor_radii = new float[MaxFloorCount];

            float[] ceiling_heights = new float[MaxFloorCount];



            //Wing data buffers

            int max_wing_count = MaxWingCountPerFloor * MaxFloorCount;
            float[] wing_indices = new float[max_wing_count];

            float[] wing_positions = new float[max_wing_count];

            float[] wing_widths = new float[max_wing_count];


            Color[] wing_colors = new Color[max_wing_count];


            //Wing data textures

            Texture2D wing_indices_texture = new Texture2D(MaxWingCountPerFloor / 4,
                                                    MaxFloorCount,
                                                    TextureFormat.RGBAFloat, false);
            wing_indices_texture.filterMode = FilterMode.Point;
            wing_indices_texture.wrapMode = TextureWrapMode.Clamp;

            Texture2D wing_positions_texture = GameObject.Instantiate(wing_indices_texture);

            Texture2D wing_widths_texture = GameObject.Instantiate(wing_indices_texture);

            Texture2D wing_colors_texture = new Texture2D(MaxWingCountPerFloor,
                                                MaxFloorCount,
                                                TextureFormat.RGBA32, false);
            wing_colors_texture.filterMode = FilterMode.Point;
            wing_colors_texture.wrapMode = TextureWrapMode.Clamp;


            //Clear WingVisualizations

            foreach (WingVisualization wing_visualization in WingVisualizations)
                GameObject.DestroyImmediate(wing_visualization.gameObject);


            //Generate data

            foreach (WingVisualization wing_visualization in WingVisualizations)
                GameObject.DestroyImmediate(wing_visualization.gameObject);

            int floor_slot_index = 0;
            int wing_index = 0;
            foreach (Ring.Floor floor in Ring.Floors)
            {
                int floor_index = Ring.Floors.IndexOf(floor);
                float ceiling_height = floor.CeilingHeight;
                float floor_radius = Ring.GroundFloorRadius - floor_slot_index * Ring.Floor.UnitCeilingHeight;

                int floor_slot_count = (ceiling_height / Ring.Floor.UnitCeilingHeight).Round();
                for (int i = 0; i < floor_slot_count; i++)
                    floor_indices[floor_slot_index++] = floor_index;
                ceiling_heights[floor_index] = ceiling_height;
                floor_radii[floor_index] = floor_radius;


                int wing_slot_index = 0;
                foreach (Ring.Floor.Wing wing in floor.Wings)
                {
                    float wing_position = wing_slot_index * Ring.UnitWingWidth;
                    float wing_width = wing.Width;
                    Color wing_color = Color.green;

                    int wing_slot_count = (wing_width / Ring.UnitWingWidth).Round();
                    for (int i = 0; i < wing_slot_count; i++)
                        wing_indices[floor_index * MaxWingCountPerFloor + wing_slot_index++] = wing_index;
                    wing_widths[wing_index] = wing_width;
                    wing_positions[wing_index] = wing_position;
                    wing_colors[wing_index] = wing_color;

                    wing_index++;


                    WingVisualization wing_visualization = 
                        GameObject.Instantiate(WingVisualizationPrefab);
                    wing_visualization.Wing = wing;
                    wing_visualization.transform.SetParent(transform);
                }
            }

            List<float> foo = wing_indices.ToList().GetRange(0, 1000);
            List<float> bar = foo;
            foo = wing_positions.ToList().GetRange(0, 200);
            foo = wing_widths.ToList().GetRange(0, 200);

            material.SetFloatArray("FloorIndices", floor_indices);
            material.SetFloatArray("FloorRadii", floor_radii);
            material.SetFloatArray("FloorHeights", ceiling_heights);

            wing_indices_texture.SetPixelData(wing_indices, 0);
            wing_indices_texture.Apply();
            material.SetTexture("WingIndices", wing_indices_texture);

            wing_positions_texture.SetPixelData(wing_positions, 0);
            wing_positions_texture.Apply();
            material.SetTexture("WingPositions", wing_positions_texture);

            wing_widths_texture.SetPixelData(wing_widths, 0);
            wing_widths_texture.Apply();
            material.SetTexture("WingWidths", wing_widths_texture);

            wing_colors_texture.SetPixels(wing_colors);
            wing_colors_texture.Apply();
            material.SetTexture("WingColors", wing_colors_texture);


            outer_radius_displayed = Ring.GroundFloorRadius;

            wings_displayed_per_floor.Clear();
            foreach (Ring.Floor floor in Ring.Floors)
                wings_displayed_per_floor.Add(floor.Wings.Count());
        }

        material.SetFloat("FloorDivisor", Ring.Floor.UnitCeilingHeight);
        material.SetFloat("InterstitialSpaceThickness", Ring.Floor.InterstitialSpaceThickness * 3);

        material.SetFloat("OuterRadius", Ring.GroundFloorRadius);
        material.SetFloat("InnerRadius", Ring.RoofRadius);

        material.SetFloat("WingDivisor", Ring.UnitWingWidth);
        material.SetFloat("WingDepth", Ring.UnitWingDepth);
        material.SetFloat("RingDepth", Ring.Depth);

        material.SetFloat("Linearity", Linearity);
        material.SetFloat("ApplyWidthCorrection", 1);
        material.SetFloat("WingVisibility", WingVisibility);
        material.SetFloat("WireframeVisibility", WireframeVisibility);

        material.SetFloat("WallThickness", Ring.Floor.Wing.WallThickness * 3);

        MeshRenderer.SetPropertyBlock(material);
    }

    public Vector3 PolarCoordinatesToPosition(float radians, float radius)
    {
        Quaternion rotation;
        return PolarCoordinatesToPosition(radians, radius, out rotation);
    }

    public Vector3 PolarCoordinatesToPosition(float radians, float radius, out Quaternion rotation)
    {
        float linearized_radians = radians * (1 - Linearity);
        float linearized_outer_radius = Ring.GroundFloorRadius / (1 - Linearity);
        float linearized_radius = radius +
                                  (linearized_outer_radius - Ring.GroundFloorRadius);

        float x = linearized_radius * Mathf.Sin(linearized_radians);
        float y = linearized_radius * Mathf.Cos(linearized_radians) -
                  (linearized_outer_radius - Ring.GroundFloorRadius);

        rotation = Quaternion.Euler(0, 0, MathUtility.RadiansToDegrees(-linearized_radians));

        return new Vector3(x, y);
    }

    public WingVisualization WingVisualizationPrefab;
}
