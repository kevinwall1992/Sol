using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InterplanetaryTransfer : Navigation.Transfer
{
    HohmannTransfer hohmann_transfer;

    public override DateTime DepartureDate
    { get { return hohmann_transfer.DepartureDate; } }

    public override DateTime ArrivalDate
    { get { return hohmann_transfer.ArrivalDate; } }

    public override List<Maneuver> Maneuvers
    {
        get
        {
            return Utility.List(
                hohmann_transfer.TransferManeuver, 
                new Maneuver(ArrivalDate, TargetMotion));
        }
    }

    public InterplanetaryTransfer(SatelliteMotion original_motion, 
                                  SatelliteMotion target_motion, 
                                  System.DateTime earliest_launch_date)
        : base(original_motion, target_motion)
    {
        SystemMapObject transfer_primary = null;
        foreach(SatelliteMotion octave in original_motion.Hierarchy)
            if(target_motion.Hierarchy.Contains(octave.Primary.Motion))
            {
                transfer_primary = octave.Primary;
                break;
            }
        Debug.Assert(transfer_primary != null);

        hohmann_transfer = new HohmannTransfer(
            OriginalMotion.Hierarchy.PreviousElement(transfer_primary.Motion),
            TargetMotion.Hierarchy.PreviousElement(transfer_primary.Motion), 
            earliest_launch_date);
    }
}
