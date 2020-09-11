using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Navigation : MonoBehaviour, Craft.Part
{
    int transfers_completed = 0;
    int maneuvers_completed_in_transfer = 0;

    public float Mass;

    public List<Transfer> Transfers = new List<Transfer>();

    public Transfer NextTransfer
    {
        get
        {
            if (Transfers.Count == 0 || 
                Transfers.Count <= transfers_completed)
                return null;

            return Transfers[transfers_completed];
        }
    }

    public Transfer.Maneuver NextManeuver
    {
        get
        {
            if (NextTransfer == null)
                return null;

            return NextTransfer.Maneuvers[maneuvers_completed_in_transfer];
        }
    }

    public float PartMass { get { return Mass; } }

    private void Start()
    {
        
    }

    void Update()
    {
        if (NextManeuver != null &&
            Scene.The.Clock.Now > NextManeuver.Date)
        {
            this.Craft().Engine.ApplyManeuver(NextManeuver);
            maneuvers_completed_in_transfer++;

            if (maneuvers_completed_in_transfer == NextTransfer.Maneuvers.Count())
            {
                transfers_completed++;
                maneuvers_completed_in_transfer = 0;
            }
        }
    }

    public void AddTransfer(Transfer transfer)
    {
        Transfers.Add(transfer);
    }

    public void RemoveTransfer(Transfer transfer)
    {
        Transfers.Remove(transfer);
    }


    public abstract class Transfer
    {
        public SatelliteMotion OriginalMotion { get; private set; }
        public SatelliteMotion TargetMotion { get; private set; }

        public abstract System.DateTime DepartureDate { get; }
        public abstract System.DateTime ArrivalDate { get; }

        public abstract List<Maneuver> Maneuvers { get; }


        public Transfer(SatelliteMotion original_motion, SatelliteMotion target_motion)
        {
            OriginalMotion = original_motion;
            TargetMotion = target_motion;
        }


        public class Maneuver
        {
            public System.DateTime Date;
            public SatelliteMotion ResultingMotion;
            
            public bool WasExecuted { get; set; }
            
            public Maneuver(System.DateTime date, 
                            SatelliteMotion resulting_motion)
            {
                Date = date;
                ResultingMotion = resulting_motion;

                WasExecuted = false;
            }

            public Maneuver()
            {
                WasExecuted = false;
            }
        }
    }
}
