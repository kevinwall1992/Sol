using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class Ring : Station.Module
{
    public List<Floor> Floors = new List<Floor>();

    public float GroundFloorRadius
    { get { return Floors.Count > 0 ? Floors.First().Radius : 0; } }
    public float RoofRadius
    { get { return GroundFloorRadius - Floors.Sum(floor => floor.CeilingHeight); } }

    public float UnitWingWidth = 18;
    public float UnitWingDepth = 10;
    public float WalkwayDepth = 1;

    public float RPM = 2;

    public int LayerCount;
    public float LayerDepth { get { return UnitWingDepth + WalkwayDepth; } }
    public float Depth { get { return LayerDepth * LayerCount; } }

    private void Update()
    {

    }

    public void GenerateSampleStructure(float ground_floor_radius, int floor_count)
    {
        Floors.Clear();

        int[] ceiling_sizes = { 20 };

        float radius = ground_floor_radius;
        for(int i = 0; i< floor_count; i++)
        {
            int ceiling_size =
                ceiling_sizes[MathUtility.RandomIndex(ceiling_sizes.Length)];
            Floor floor = new Floor(this, radius, ceiling_size);
            Floors.Add(floor);

            float meters = 0;
            while(true)
            {
                int wing_size = MathUtility.RandomIndex(3) + 1;
                float wing_width = wing_size * UnitWingWidth;

                if (meters + wing_width > floor.ArcLength)
                    break;

                floor.Wings.Add(new Floor.Wing(floor, meters, wing_size));

                meters += wing_width;
            }

            radius -= floor.CeilingHeight;
        }
    }

    public Floor GetFloor(float Radius)
    {
        return Floors.FirstOrDefault(floor => 
            floor.Radius > Radius && 
            floor.Radius - floor.CeilingHeight < Radius);
    }

    public Floor.Wing GetWing(float Radians, float Radius)
    {
        Floor floor = GetFloor(Radius);
        if (floor == null)
            return null;

        return floor.GetWing(Radians);
    }

    public class Floor
    {
        public Ring Ring { get; private set; }

        public List<Wing> Wings = new List<Wing>();

        public float Radius;

        public int CeilingSize;
        public float CeilingHeight
        { get { return CeilingSize * UnitCeilingHeight; } }

        public float ArcLength { get { return Radius * 2 * Mathf.PI; } }


        public Floor(Ring ring, float radius, int ceiling_size = 5)
        {
            Ring = ring;

            Radius = radius;
            CeilingSize = ceiling_size;
        }

        public Wing GetWing(float Radians)
        {
            return Wings.FirstOrDefault(wing =>
                wing.StartRadians < Radians &&
                wing.EndRadians > Radians);
        }

        public float MetersToRadians(float meters)
        { return meters / ArcLength * 2 * Mathf.PI; }

        public float RadiansToMeters(float radians)
        { return radians / (2 * Mathf.PI) * ArcLength;}


        public static float UnitCeilingHeight = 0.4f;
        public static float InterstitialSpaceThickness = 0.3f;


        public class Wing
        {
            Floor floor;
            public Floor Floor { get { return floor; } }

            public float Position;
            public int Size;

            public float Width { get { return Size * Floor.Ring.UnitWingWidth; } }
            public float Height { get { return Floor.CeilingHeight; } }
            public float Depth { get { return Floor.Ring.UnitWingDepth * Floor.Ring.LayerCount; } }

            public float Area { get { return Width * Depth; } }

            public float StartRadians
            { get { return Floor.MetersToRadians(Position); } }

            public float EndRadians
            { get { return StartRadians + RadianWidth; } }

            public float RadianCenter
            { get { return StartRadians + RadianWidth / 2; } }

            public float RadianWidth
            { get { return Floor.MetersToRadians(Width); } }

            public Wing(Floor floor_, float position, int size = 1)
            {
                floor = floor_;

                Position = position;
                Size = size;
            }


            public static float WallThickness = 0.3f;
        }
    }
}
