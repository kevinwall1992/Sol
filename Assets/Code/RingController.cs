using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RingController : MonoBehaviour
{
    float[] floor_indices;
    float[] floor_radii;
    float[] floor_heights;

    float[] room_indices;
    float[] room_positions;
    float[] room_lengths;
    Color[] room_colors;

    Texture2D room_indices_texture;
    Texture2D room_positions_texture;
    Texture2D room_lengths_texture;
    Texture2D room_colors_texture;

    public float OuterRadius;
    public float LayerCount;

    public float UnitFloorHeight;
    public int FloorHeight;
    public int FloorCount;
    public float FloorMargin;

    public float UnitRoomLength;
    public int RoomLength;
    public float RoomDepth;
    public float RoomMargin;

    [Range(0, 0.9999f)]
    public float Linearity = 0;
    [Range(0, 1)]
    public float Rationality = 0;
    [Range(0, 1)]
    public float Coloration = 0;

    public MeshRenderer MeshRenderer;

    public int FloorResolution = 256;
    public int MaxFloorCount = 128;
    public int MaxRoomCountPerFloor = 1024;

    public bool Regenerate = true;

    void Update()
    {
        MaterialPropertyBlock material = new MaterialPropertyBlock();
        MeshRenderer.GetPropertyBlock(material);

        if (Regenerate)
        {
            Regenerate = false;


            //Floor data buffers

            floor_indices = new float[FloorResolution];

            floor_radii = new float[MaxFloorCount];

            floor_heights = new float[MaxFloorCount];



            //Room data buffers

            int max_room_count = MaxRoomCountPerFloor * MaxFloorCount;
            room_indices = new float[max_room_count];

            room_positions = new float[max_room_count];

            room_lengths = new float[max_room_count];


            room_colors = new Color[max_room_count];


            //Room data textures

            room_indices_texture = new Texture2D(MaxRoomCountPerFloor / 4,
                                                    MaxFloorCount,
                                                    TextureFormat.RGBAFloat, false);
            room_indices_texture.filterMode = FilterMode.Point;
            room_indices_texture.wrapMode = TextureWrapMode.Clamp;

            room_positions_texture = GameObject.Instantiate(room_indices_texture);

            room_lengths_texture = GameObject.Instantiate(room_indices_texture);

            room_colors_texture = new Texture2D(MaxRoomCountPerFloor,
                                                MaxFloorCount,
                                                TextureFormat.RGBA32, false);
            room_colors_texture.filterMode = FilterMode.Point;
            room_colors_texture.wrapMode = TextureWrapMode.Clamp;



            //Generate data

            for (int i = 0; i < MaxFloorCount; i++)
                floor_heights[i] = (FloorHeight * (1 + Random.value * 1.5f - 0.5f)).Round();

            for (int i = 0; i < MaxRoomCountPerFloor; i++)
                room_lengths[i] = MathUtility.RandomIndex(3) + 1;

            int floor_slot_index = 0;
            int room_index = 0;

            for (int floor_index = 0; floor_index < FloorCount; floor_index++)
            {
                float floor_radius = OuterRadius -
                                     floor_slot_index * UnitFloorHeight;
                floor_radii[floor_index] = floor_radius;
                float floor_length = floor_radius * 2 * Mathf.PI;

                int floor_height = floor_heights[floor_index].Round();

                for (int i = 0; i < floor_height; i++)
                    floor_indices[floor_slot_index++] = floor_index;


                float meters = 0;
                int room_slot_index = 0;

                while (true)
                {
                    int room_length = room_lengths[room_index].Round();
                    if ((room_length * UnitRoomLength + meters) > floor_length)
                        break;

                    for (int i = 0; i < room_length; i++)
                        room_indices[floor_index * MaxRoomCountPerFloor +
                                     room_slot_index++] = room_index;

                    room_positions[room_index] = meters;
                    room_colors[room_index] =
                        new Color(Random.value, Random.value, Random.value);

                    room_index++;
                    meters += room_length * UnitRoomLength;
                }
            }

            room_indices_texture.SetPixelData(room_indices, 0);
            room_indices_texture.Apply();
            room_positions_texture.SetPixelData(room_positions, 0);
            room_positions_texture.Apply();
            room_lengths_texture.SetPixelData(room_lengths, 0);
            room_lengths_texture.Apply();
            room_colors_texture.SetPixels(room_colors);
            room_colors_texture.Apply();

            material.SetFloat("InnerRadius", OuterRadius - floor_slot_index * UnitFloorHeight);
        }

        material.SetFloat("UnitFloorHeight", UnitFloorHeight);
        material.SetFloat("FloorMargin", FloorMargin);

        material.SetFloat("OuterRadius", OuterRadius);
        
        material.SetFloat("Thickness", LayerCount * RoomDepth);

        material.SetFloat("Linearity", Linearity);
        material.SetFloat("Rationality", Rationality);
        material.SetFloat("Coloration", Coloration);

        material.SetFloat("UnitRoomLength", UnitRoomLength);
        material.SetFloat("RoomMargin", RoomMargin);


        material.SetFloatArray("FloorIndices", floor_indices);
        material.SetFloatArray("FloorRadii", floor_radii);
        material.SetFloatArray("FloorHeights", floor_heights);


        material.SetTexture("RoomIndices", room_indices_texture);
        material.SetTexture("RoomPositions", room_positions_texture);
        material.SetTexture("RoomLengths", room_lengths_texture);
        material.SetTexture("RoomColors", room_colors_texture);

        MeshRenderer.SetPropertyBlock(material);
    }
}
