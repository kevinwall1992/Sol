using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RingVisualization : MonoBehaviour
{
    MaterialPropertyBlock material;
    Texture2D wing_colors_texture;
    Color[] wing_colors;

    bool in_linear_transition = false;
    float start_degrees = 0;

    float outer_radius_displayed = 0;
    List<int> wings_displayed_per_floor = new List<int>();

    public Ring Ring;

    public MeshRenderer MeshRenderer;
    public Transform WingVisualizationsContainer;

    public float Linearity;
    public float WingVisibility;
    public float WireframeVisibility;

    public Color Color;

    public int FloorResolution = 256;
    public int MaxFloorCount = 128;
    public int MaxWingCountPerFloor = 1024;

    public Vector3 Front { get { return new Vector3(0, 0, 1); } }
    public Vector3 Seam { get { return new Vector3(0, 1, 0); } }

    public bool IsSelected
    { get { return StationVisualization.SelectedRing == this; } }

    public IEnumerable<WingVisualization> WingVisualizations
    { get { return GetComponentsInChildren<WingVisualization>(); } }

    public StationVisualization StationVisualization
    { get { return GetComponentInParent<StationVisualization>(); } }

    void Update()
    {
        if (Ring.Floors.Count() == 0)
            Ring.GenerateSampleStructure(223, 5);


        //Animation

        if (Application.isPlaying)
        {
            //Spin

            if (Linearity == 0)
            {
                in_linear_transition = false;

                transform.rotation = Quaternion.Euler(0, 0,
                    transform.rotation.eulerAngles.z +
                    Ring.RPM * 360 / 60.0f * Time.deltaTime);
            }
            else
            {
                if (!in_linear_transition)
                {
                    start_degrees = transform.rotation.eulerAngles.z;
                    in_linear_transition = true;
                }

                transform.rotation = Quaternion.Euler(0, 0,
                    Mathf.Lerp(start_degrees, 0, Linearity));
            }
        }


        //Input

        if(IsSelected)
        {
            Ray ray = Scene.The.StationViewer.GetRayFromCursorPosition();
            Vector2 polar_coordinates = PolarCoordinatesFromRay(ray);

            Ring.Floor.Wing wing_pointed_at = 
                Ring.GetWing(polar_coordinates.x, polar_coordinates.y);

            foreach (WingVisualization wing in WingVisualizations)
                if (wing.Wing == wing_pointed_at)
                    wing.Color = Color.yellow;
                else
                    wing.Color = Color.green;
        }


        //Shader 

        if (material == null)
        {
            material = new MaterialPropertyBlock();
            MeshRenderer.GetPropertyBlock(material);
        }

        bool regenerate = false;
        if (outer_radius_displayed != Ring.GroundFloorRadius)
            regenerate = true;

        if (wings_displayed_per_floor.Count != Ring.Floors.Count)
            regenerate = true;
        else
            for (int i = 0; i < Ring.Floors.Count; i++)
                if (Ring.Floors[i].Wings.Count != wings_displayed_per_floor[i])
                    regenerate = true;

        if (regenerate && Application.isPlaying)
        {
            //Floor data buffers

            float[] floor_indices = new float[FloorResolution];

            float[] floor_radii = new float[MaxFloorCount];

            float[] ceiling_heights = new float[MaxFloorCount];



            //Wing data buffers

            int max_wing_count = MaxWingCountPerFloor * MaxFloorCount;
            float[] wing_indices = new float[max_wing_count];
            for (int i = 0; i < max_wing_count; i++)
                wing_indices[i] = -1;

            float[] wing_positions = new float[max_wing_count];

            float[] wing_widths = new float[max_wing_count];


            //Wing data textures

            Texture2D wing_indices_texture = new Texture2D(MaxWingCountPerFloor / 4,
                                                    MaxFloorCount,
                                                    TextureFormat.RGBAFloat, false);
            wing_indices_texture.filterMode = FilterMode.Point;
            wing_indices_texture.wrapMode = TextureWrapMode.Clamp;

            Texture2D wing_positions_texture = GameObject.Instantiate(wing_indices_texture);

            Texture2D wing_widths_texture = GameObject.Instantiate(wing_indices_texture);


            //Clear WingVisualizations

            foreach (WingVisualization wing in WingVisualizations)
                GameObject.DestroyImmediate(wing.gameObject);


            //Generate data

            foreach (WingVisualization wing in WingVisualizations)
                GameObject.DestroyImmediate(wing.gameObject);

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

 
                foreach (Ring.Floor.Wing wing in floor.Wings)
                {
                    float wing_width = wing.Width;
                    Color wing_color = Color;

                    int wing_slot_count = (wing_width / Ring.UnitWingWidth).Round();
                    int wing_slot_start_index = (wing.Position / Ring.UnitWingWidth).Round();
                    for (int i = 0; i < wing_slot_count; i++)
                        wing_indices[floor_index * MaxWingCountPerFloor + 
                                     wing_slot_start_index + i] = wing_index;
                    wing_widths[wing_index] = wing_width;
                    wing_positions[wing_index] = wing.Position;

                    wing_index++;


                    WingVisualization wing_visualization = 
                        GameObject.Instantiate(WingVisualizationPrefab);
                    wing_visualization.Wing = wing;
                    wing_visualization.transform.SetParent(WingVisualizationsContainer);
                    wing_visualization.Color = Color;
                }
            }

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


            outer_radius_displayed = Ring.GroundFloorRadius;

            wings_displayed_per_floor.Clear();
            foreach (Ring.Floor floor in Ring.Floors)
                wings_displayed_per_floor.Add(floor.Wings.Count());
        }

        UpdateWingColors();

        material.SetFloat("FloorDivisor", Ring.Floor.UnitCeilingHeight);
        material.SetFloat("InterstitialSpaceThickness", 
            Ring.Floor.InterstitialSpaceThickness * Linearity * 2);

        material.SetFloat("OuterRadius", Ring.GroundFloorRadius);
        material.SetFloat("InnerRadius", Ring.RoofRadius);

        material.SetFloat("WingDivisor", Ring.UnitWingWidth);
        material.SetFloat("WingDepth", Ring.UnitWingDepth);
        material.SetFloat("RingDepth", Ring.Depth);

        material.SetFloat("Linearity", Linearity);
        material.SetFloat("ApplyWidthCorrection", 1);
        material.SetFloat("WingVisibility", WingVisibility);
        material.SetFloat("WireframeVisibility", WireframeVisibility);
        material.SetColor("WireFrameColor", Color);

        material.SetFloat("WallThickness", 
            Ring.Floor.Wing.WallThickness * Linearity * 2);

        MeshRenderer.SetPropertyBlock(material);
    }

    void UpdateWingColors()
    {
        if (!UnityEditor.EditorApplication.isPlaying)
            return;

        if (wing_colors_texture == null || 
            wing_colors_texture.width != MaxWingCountPerFloor ||
            wing_colors_texture.height != MaxFloorCount)
        {
            if (wing_colors_texture != null)
                GameObject.Destroy(wing_colors_texture);

            wing_colors_texture = new Texture2D(MaxWingCountPerFloor,
                                                MaxFloorCount,
                                                TextureFormat.RGBA32, false);

            wing_colors_texture.filterMode = FilterMode.Point;
            wing_colors_texture.wrapMode = TextureWrapMode.Clamp;

            wing_colors = new Color[MaxWingCountPerFloor * MaxFloorCount];
        }

        foreach (WingVisualization wing in WingVisualizations)
        {
            int wing_index =
                Ring.Floors.PreviousElements(wing.Wing.Floor)
                    .Sum(floor => floor.Wings.Count()) +
                wing.Wing.Floor.Wings.IndexOf(wing.Wing);
            
            wing_colors[wing_index] = wing.Color;
        }

        wing_colors_texture.SetPixels(wing_colors);
        wing_colors_texture.Apply();
        material.SetTexture("WingColors", wing_colors_texture);
    }

    //Position is in RingVisualization space
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

    public Vector2 PositionToPolarCoordinates(Vector3 position)
    {
        float linearized_outer_radius = Ring.GroundFloorRadius / (1 - Linearity);

        float linearized_radians = Mathf.Atan2(
            position.x, 
            (position.y + linearized_outer_radius - Ring.GroundFloorRadius));

        float radius = position.x / Mathf.Sin(linearized_radians) - 
                       (linearized_outer_radius - Ring.GroundFloorRadius);

        float radians = linearized_radians / (1 - Linearity);
        if (radians < 0)
            radians += 2 * Mathf.PI;

        return new Vector2(radians, radius);
    }

    //Ray is in StationVisualization space
    public Vector2 PolarCoordinatesFromRay(Ray ray)
    {
        Vector3 intersection = 
            ray.Intersect(new Plane(new Vector3(0, 0, 1), transform.localPosition));


        //Intersection must be in RingVisualization space
        intersection = StationVisualization.transform.TransformPoint(intersection);
        intersection = transform.InverseTransformPoint(intersection);

        return PositionToPolarCoordinates(intersection);
    }

    public bool Occludes(Ray ray, float tolerance)
    {
        if (Linearity > 0)
            return true;

        System.Func<float, bool> Occludes = offset =>
        {
            Vector3 position = transform.localPosition + 
                               new Vector3(0, 0, offset);

            Vector3 polar_coordinates = 
                MathUtility.PolarCoordinates(Front, position, Seam, ray);

            float meter_tolerance = (Ring.GroundFloorRadius - Ring.RoofRadius) * tolerance;

            return polar_coordinates.y <= Ring.GroundFloorRadius + meter_tolerance && 
                   polar_coordinates.y > Ring.RoofRadius - meter_tolerance;
        };

        if (Occludes(0) || Occludes(Ring.Depth))
            return true;

        return false;
    }

    public WingVisualization WingVisualizationPrefab;
}
