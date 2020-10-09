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

    public List<Transfer> UpcomingTransfers
    {
        get
        {
            if (transfers_completed >= Transfers.Count)
                return new List<Transfer>();

            return Transfers.GetRange(transfers_completed, 
                                      Transfers.Count - transfers_completed);
        }
    }

    public Transfer NextTransfer
    {
        get
        {
            if (UpcomingTransfers.Count == 0)
                return null;  

            return UpcomingTransfers.First();
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

    public Engine Engine { get { return this.Craft().Engine; } }

    private void Start()
    {
        
    }

    void Update()
    {
        if (NextManeuver != null &&
            Scene.The.Clock.Now > NextManeuver.Date)
        {
            if (maneuvers_completed_in_transfer == 0)
            {
                float propellent_mass_required = 
                    1.001f *
                    Engine.GetPropellentMassRequired(NextTransfer,
                                                     this.Craft().EmptyTankMass);

                if (Engine.PropellentMass < propellent_mass_required)
                {
                    bool sucessfully_refueled = false;

                    if (this.Craft().Station != null)
                        sucessfully_refueled = Engine.Refuel(
                            propellent_mass_required, 
                            this.Craft().Station.OfficialMarket);

                    if(!sucessfully_refueled)
                    {
                        int index = Transfers.IndexOf(NextTransfer);

                        Transfers.RemoveRange(index, Transfers.Count - index);
                        return;
                    }
                }
            }

            Engine.ApplyManeuver(NextManeuver);
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
