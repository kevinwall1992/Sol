using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class SatelliteMotion : Motion
{
    public Satellite Primary;

    public float Periapsis;
    public float Apoapsis;
    public float ArgumentOfPeriapsis;
    public float MeanAnomalyAtEpoch;


    public float SemimajorAxis
    { get { return (Apoapsis + Periapsis) / 2; } }

    public float SemiminorAxis
    { get { return (Apoapsis + Periapsis) / 2; } }

    public float Eccentricity
    { get { return Apoapsis / SemimajorAxis - 1; } }

    public float Period
    {
        get
        {
            return 2 * Mathf.PI *
                   Mathf.Sqrt(Mathf.Pow(SemimajorAxis, 3) /
                   (MathConstants.GravitationalConstant * Primary.Mass));
        }
    }

    public float AverageAltitude
    { get { return SemimajorAxis * (1 + Mathf.Pow(Eccentricity, 2) / 2); } }

    public float MeanAnomaly
    { get { return MeanAnomalyAtDate(The.Clock.Now); } }

    public float EccentricAnomaly
    { get { return EccentricAnomalyGivenMeanAnomaly(MeanAnomaly); } }

    public float TrueAnomaly
    { get { return TrueAnomalyGivenMeanAnomaly(MeanAnomaly); } }

    public float DistanceFromPrimary
    { get { return AltitudeGivenTrueAnomaly(TrueAnomaly); } }

    //Velocity with respect to Primary
    public Vector3 LocalVelocity
    { get { return LocalVelocityGivenTrueAnomaly(TrueAnomaly); } }

    //Velocity with respect to top level primary
    public Vector3 Velocity
    { get { return VelocityAtDate(The.Clock.Now); } }

    public float PathLength
    { get { return MathUtility.EllipseCircumference(SemimajorAxis, Eccentricity); } }

    public Vector3 Position
    { get { return PositionAtDate(The.Clock.Now); } }

    public List<SatelliteMotion> Hierarchy
    {
        get
        {
            List<SatelliteMotion> hierarchy = new List<SatelliteMotion>();
            hierarchy.Add(this);

            while (hierarchy.Last().Primary != null)
                hierarchy.Add(hierarchy.Last().Primary.Motion);

            return hierarchy;
        }
    }

    public SatelliteMotion(Satellite primary,
                           float periapsis, 
                           float apoapsis, 
                           float argument_of_periapsis, 
                           float mean_anomaly_at_epoch)
    {
        Primary = primary;

        Apoapsis = apoapsis;
        Periapsis = periapsis;
        ArgumentOfPeriapsis = argument_of_periapsis;
        MeanAnomalyAtEpoch = mean_anomaly_at_epoch;
    }

    public SatelliteMotion()
    {

    }


    public float MeanAnomalyAtDate(System.DateTime date)
    {
        return (float)(2 * Mathf.PI *
               ((The.Clock.DateToSecondsSinceEpoch(date) / Period) % 1) +
               MeanAnomalyAtEpoch);
    }

    public float EccentricAnomalyGivenMeanAnomaly(float mean_anomaly)
    {
        return MathUtility.Root(
            x => x - Eccentricity * Mathf.Sin(x) - mean_anomaly,
            x => 1 - Eccentricity * Mathf.Cos(x),
            1e-4f,
            mean_anomaly);

    }

    public float EccentricAnomalyGivenTrueAnomaly(float true_anomaly)
    {
        return 2 * Mathf.Atan(Mathf.Tan(true_anomaly / 2) /
                              Mathf.Sqrt((1 + Eccentricity) / (1 - Eccentricity)));
    }

    public float TrueAnomalyGivenMeanAnomaly(float mean_anomaly)
    {
        return 2 * Mathf.Atan(Mathf.Sqrt((1 + Eccentricity) / (1 - Eccentricity)) *
                              Mathf.Tan(EccentricAnomalyGivenMeanAnomaly(mean_anomaly) / 2));
    }

    public float TrueAnomalyAtDate(System.DateTime date)
    {
        return TrueAnomalyGivenMeanAnomaly(MeanAnomalyAtDate(date));
    }

    public float AltitudeGivenTrueAnomaly(float true_anomaly)
    {
        return SemimajorAxis * (1 - Mathf.Pow(Eccentricity, 2)) /
               (1 + Eccentricity * Mathf.Cos(true_anomaly));
    }

    public float AltitudeAtDate(System.DateTime date)
    {
        return AltitudeGivenTrueAnomaly(TrueAnomalyAtDate(date));
    }

    public Vector3 LocalVelocityGivenTrueAnomaly(float true_anomaly)
    {
        float eccentric_anomaly = EccentricAnomalyGivenTrueAnomaly(true_anomaly);

        Vector3 velocity = 
            Mathf.Sqrt(MathConstants.GravitationalConstant * Primary.Mass * SemimajorAxis) /
            AltitudeGivenTrueAnomaly(true_anomaly) *
            new Vector3(-Mathf.Sin(eccentric_anomaly),
                        Mathf.Sqrt(1 - Mathf.Pow(Eccentricity, 2)) * Mathf.Cos(eccentric_anomaly));

        return new Vector3(velocity.x * Mathf.Cos(ArgumentOfPeriapsis) - 
                           velocity.y * Mathf.Sin(ArgumentOfPeriapsis),

                           velocity.y * Mathf.Cos(ArgumentOfPeriapsis) + 
                           velocity.x * Mathf.Sin(ArgumentOfPeriapsis));
    }

    public Vector3 LocalVelocityAtDate(System.DateTime date)
    {
        return LocalVelocityGivenTrueAnomaly(TrueAnomalyAtDate(date));
    }

    public Vector3 EquatorialVelocityGivenTrueAnomaly(float true_anomaly)
    {
        Vector3 velocity = LocalVelocityGivenTrueAnomaly(true_anomaly);

        return new Vector3(
            -(velocity.y * Mathf.Cos(true_anomaly) + velocity.x * Mathf.Sin(true_anomaly)),
            velocity.x * Mathf.Cos(true_anomaly) - velocity.y * Mathf.Sin(true_anomaly),
            0);
    }

    public Vector3 VelocityAtDate(System.DateTime date)
    {
        return LocalVelocityAtDate(date) + 
               (Primary != null ? Primary.Motion.Velocity : 
                                  Vector3.zero);
    }

    public Vector3 LocalPositionGivenMeanAnomaly(float mean_anomaly)
    {
        return LocalPositionGivenTrueAnomaly(TrueAnomalyGivenMeanAnomaly(mean_anomaly));
    }

    public Vector3 LocalPositionGivenTrueAnomaly(float true_anomaly)
    {
        if (Primary == null)
            return Vector3.zero;

        return new Vector3(Mathf.Sin(-(true_anomaly + ArgumentOfPeriapsis) + Mathf.PI / 2),
                           Mathf.Cos(-(true_anomaly + ArgumentOfPeriapsis) + Mathf.PI / 2), 0) *
               AltitudeGivenTrueAnomaly(true_anomaly);
    }

    public Vector3 LocalPositionAtDate(System.DateTime date)
    {
        return LocalPositionGivenMeanAnomaly(MeanAnomalyAtDate(date));
    }

    public Vector3 PositionAtDate(System.DateTime date)
    {
        if (Primary == null)
            return Vector3.zero;

        return Primary.Motion.PositionAtDate(date) +
               LocalPositionGivenMeanAnomaly(MeanAnomalyAtDate(date));
    }
}
