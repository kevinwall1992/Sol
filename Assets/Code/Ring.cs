using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class Ring : Station.Module
{
    public List<Floor> Floors = new List<Floor>();
    public float GroundFloorRadius;
    public float RoofRadius
    { get { return GroundFloorRadius - Floors.Sum(floor => floor.CeilingHeight); } }

    public float UnitWingWidth = 6;
    public float UnitWingDepth = 10;
    public float WalkwayDepth = 1;

    public int LayerCount;
    public float LayerDepth { get { return UnitWingDepth + WalkwayDepth; } }
    public float Depth { get { return LayerDepth * LayerCount; } }

    private void Update()
    {
        if (Floors.Count == 0)
            GenerateSampleStructure(5);
    }

    public void GenerateSampleStructure(int floor_count)
    {
        Floors.Clear();

        int[] ceiling_sizes = { 5, 10, 15 };

        for(int i = 0; i< floor_count; i++)
        {
            int ceiling_size =
                ceiling_sizes[MathUtility.RandomIndex(ceiling_sizes.Length)];
            Floor floor = new Floor(this, ceiling_size);
            Floors.Add(floor);

            float meters = 0;
            while(true)
            {
                int wing_size = MathUtility.RandomIndex(3) + 1;
                float wing_width = wing_size * UnitWingWidth;

                if (meters + wing_width > floor.ArcLength)
                    break;

                floor.Wings.Add(new Floor.Wing(floor, wing_size));

                meters += wing_width;
            }
        }
    }

    public class Floor
    {
        Ring ring;
        public Ring Ring { get { return ring; } }

        public List<Wing> Wings = new List<Wing>();

        public int CeilingSize;
        public float CeilingHeight
        { get { return CeilingSize * UnitCeilingHeight; } }

        public float Radius
        {
            get
            {
                float distance_from_ground_floor =
                    Ring.Floors.GetRange(0, ring.Floors.IndexOf(this))
                    .Sum(floor => floor.CeilingHeight);

                return ring.GroundFloorRadius - distance_from_ground_floor;
            }
        }

        public float ArcLength { get { return Radius * 2 * Mathf.PI; } }


        public Floor(Ring ring_, int ceiling_size = 5)
        {
            ring = ring_;

            CeilingSize = ceiling_size;
        }

        
        public static float UnitCeilingHeight = 0.4f;
        public static float InterstitialSpaceThickness = 0.3f;


        public class Wing
        {
            Floor floor;
            public Floor Floor { get { return floor; } }

            public int Size;

            public float Width { get { return Size * Floor.Ring.UnitWingWidth; } }
            public float Height { get { return Floor.CeilingHeight; } }
            public float Depth { get { return Floor.Ring.UnitWingDepth * Floor.Ring.LayerCount; } }

            public float Area { get { return Width * Depth; } }

            public float Radians
            {
                get
                {
                    int index = Floor.Wings.IndexOf(this);
                    float distance_along_arc =
                        Floor.Wings.GetRange(0, index).Sum(wing => wing.Width) +
                        Width / 2;

                    return (distance_along_arc / Floor.ArcLength) *
                           (2 * Mathf.PI);
                }
            }

            public Wing(Floor floor_, int size = 1)
            {
                floor = floor_;

                Size = size;
            }


            public static float WallThickness = 0.3f;
        }
    }
}
