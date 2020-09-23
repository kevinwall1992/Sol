using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class HohmannTransfer : Navigation.Transfer
{
    public override System.DateTime DepartureDate { get; }

    public SatelliteMotion TransferMotion
    {
        get
        {
            Debug.Assert(TargetMotion.Periapsis == TargetMotion.Apoapsis);

            SatelliteMotion transfer_motion = new SatelliteMotion();
            transfer_motion.Primary = TargetMotion.Primary;

            Vector3 departure_position = OriginalMotion.LocalPositionAtDate(DepartureDate);
            float departure_altitude = OriginalMotion.AltitudeAtDate(DepartureDate);

            if (TargetMotion.DistanceFromPrimary > departure_altitude)
            {
                transfer_motion.Apoapsis = TargetMotion.DistanceFromPrimary;
                transfer_motion.Periapsis = departure_altitude;

                transfer_motion.ArgumentOfPeriapsis = 
                    Mathf.Atan2(departure_position.y, departure_position.x);

                transfer_motion.MeanAnomalyAtEpoch = (float)(2 * Mathf.PI * 
                    (0 - (Scene.The.Clock.DateToSecondsSinceEpoch(DepartureDate) /
                          transfer_motion.Period) % 1));
            }
            else
            {
                transfer_motion.Apoapsis = departure_altitude;
                transfer_motion.Periapsis = TargetMotion.DistanceFromPrimary;

                transfer_motion.ArgumentOfPeriapsis =
                    Mathf.Atan2(-departure_position.y, -departure_position.x);

                transfer_motion.MeanAnomalyAtEpoch = (float)(2 * Mathf.PI * 
                    (0.5f - (Scene.The.Clock.DateToSecondsSinceEpoch(DepartureDate) /
                             transfer_motion.Period) % 1));
            }

            return transfer_motion;
        }
    }

    public override System.DateTime ArrivalDate
    { get { return DepartureDate.AddSeconds(TransferMotion.Period / 2); } }

    public Maneuver TransferManeuver
    { get { return new Maneuver(DepartureDate, TransferMotion); } }

    public Maneuver InsertionManeuver
    { get { return new Maneuver(ArrivalDate, TargetMotion); } }

    public override List<Maneuver> Maneuvers
    { get { return Utility.List(TransferManeuver, InsertionManeuver); } }


    public HohmannTransfer(SatelliteMotion original_motion,
                           SatelliteMotion target_motion,
                           System.DateTime earliest_departure_date)
        : base(original_motion, target_motion)
    {
        Debug.Assert(original_motion.Primary == target_motion.Primary);

        DepartureDate = GetLaunchWindow(earliest_departure_date);
    }

    float GetRelativeRadialVelocity()
    {
        return 2 * Mathf.PI / TargetMotion.Period -
               2 * Mathf.PI / OriginalMotion.Period;
    }

    public float GetLaunchWindowPeriod()
    {
        return 2 * Mathf.PI / Mathf.Abs(GetRelativeRadialVelocity());
    }

    public System.DateTime GetLaunchWindow(System.DateTime earliest_launch, 
                                           int launch_window_skip_count = 0)
    {
        //Can't use .TransferMotion here because this method is 
        //used to determine DepartureDate, which is required for 
        //calculating .TransferMotion

        SatelliteMotion simple_transfer_motion =
            new SatelliteMotion(TargetMotion.Primary,
                                OriginalMotion.DistanceFromPrimary,
                                TargetMotion.DistanceFromPrimary,
                                0, 0);

        if (simple_transfer_motion.Periapsis > simple_transfer_motion.Apoapsis)
            Utility.Swap(ref simple_transfer_motion.Periapsis,
                         ref simple_transfer_motion.Apoapsis);

        float radial_launch_window =
            Mathf.PI -
            (2 * Mathf.PI) *
            (simple_transfer_motion.Period / 2) /
            TargetMotion.Period;

        float seconds_until_launch_window =
            (radial_launch_window - 
            TargetMotion.TrueAnomalyAtDate(earliest_launch) +
            OriginalMotion.TrueAnomalyAtDate(earliest_launch)) /
            GetRelativeRadialVelocity();
        while (seconds_until_launch_window < 0)
            seconds_until_launch_window += GetLaunchWindowPeriod();

        System.DateTime launch_date =
            earliest_launch.AddSeconds(seconds_until_launch_window);

        launch_date.AddSeconds(GetLaunchWindowPeriod() * launch_window_skip_count);

        return launch_date;
    }
}
