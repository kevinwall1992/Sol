using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using System.Linq;


public class Engine : Craft.Part
{
    public Item Propellent;
    public float ExhaustVelocity;

    public Craft Craft { get { return this.Craft(); } }

    public float PropellentMass
    {
        get
        {
            return Craft.Cargo.GetQuantity(Propellent) * Propellent.GetMassPerUnit();
        }
    }

    public float MaximumPropellentMass
    {
        get
        {
            return Craft.Cargo.GetSize(Propellent.Type);
        }
    }

    public float GetVelocityChangeAvailable()
    {
        return ExhaustVelocity *
                Mathf.Log(Craft.Mass / (Craft.Mass - PropellentMass));
    }

    public float GetMaximumVelocityChange()
    {
        float full_tank_craft_mass = (Craft.Mass - PropellentMass) + MaximumPropellentMass;

        return ExhaustVelocity *
                Mathf.Log(full_tank_craft_mass /
                          (full_tank_craft_mass - MaximumPropellentMass));
    }

    //This method simply calculates the necessary dV and then instantaneously 
    //enters the new orbital path.
    //For maneuvers that change the craft's primary, this will alter the 
    //length of the journey from reality. However, given the extreme length
    //of interplanetary journeys compared the short time spent escaping the
    //gravity well, this should still be a very good approximation. 

    public void ApplyManeuver(Navigation.Transfer.Maneuver maneuver)
    {
        float propellent_mass = 
            GetPropellentMassLossRequired(maneuver, Craft.Motion, Craft.Mass);

        if (PropellentMass < propellent_mass)
            return;

        Craft.Cargo.TakeOut(
            Propellent,
            propellent_mass / Propellent.Physical().MassPerUnit);

        Craft.Motion = maneuver.ResultingMotion;
    }

    //How much mass must be lost from craft_mass to push the remainder
    public float GetPropellentMassLossRequired(Navigation.Transfer.Maneuver maneuver,
                                             SatelliteMotion craft_motion,
                                             float craft_mass)
    {
        float payload_fraction = Mathf.Pow(
            (float)System.Math.E,
            -GetVelocityChangeRequired(maneuver, craft_motion) / ExhaustVelocity);

        return craft_mass * (1 - payload_fraction);
    }
    
    //How much additional mass is required to push craft_mass
    public float GetPropellentMassRequired(Navigation.Transfer.Maneuver maneuver, 
                                         SatelliteMotion craft_motion, 
                                         float craft_mass)
    {
        float payload_fraction = Mathf.Pow(
            (float)System.Math.E,
            -GetVelocityChangeRequired(maneuver, craft_motion) / ExhaustVelocity);

        return craft_mass / (1 / (1 - payload_fraction) - 1);
    }

    public float GetPropellentMassRequired(Navigation.Transfer transfer,
                                         float craft_mass)
    {
        float total_propellent_mass = 0;

        List<Navigation.Transfer.Maneuver> maneuvers = transfer.Maneuvers;
        foreach (Navigation.Transfer.Maneuver maneuver in maneuvers.Reversed())
        {
            int maneuver_index = maneuvers.IndexOf(maneuver);

            SatelliteMotion craft_motion;
            if (maneuver_index > 0)
                craft_motion = maneuvers[maneuver_index - 1].ResultingMotion;
            else
                craft_motion = transfer.OriginalMotion;

            total_propellent_mass +=
                GetPropellentMassRequired(maneuver, 
                                          craft_motion, 
                                          craft_mass + total_propellent_mass);
        }

        return total_propellent_mass;
    }

    public float GetVelocityChangeRequired(Navigation.Transfer.Maneuver maneuver,
                                           SatelliteMotion craft_motion)
    {
        //If craft's primary does not change, just take the difference in
        //velocities between current and target motion at the date of the 
        //maneuver

        if (maneuver.ResultingMotion.Primary == Craft.Primary)
            return (maneuver.ResultingMotion.LocalVelocityAtDate(maneuver.Date) -
                    craft_motion.LocalVelocityAtDate(maneuver.Date))
                    .magnitude;

        //In the more complex case of changing your orbit to another body,
        //this problem is solved by calculating total dV needed to escape n levels 
        //(i.e. escaping lunar orbit to solar orbit is going down 2 levels) of 
        //orbit and achieve the correct final relative velocity between craft 
        //immediately before and after maneuver. Because of reversiblity of 
        //orbits, the same dV is required for insertion (f.e. inserting into lunar
        //orbit from solar orbit) Therefore, I simply figure out which side would 
        //be prior to escape ("innermost") and which would be after escape 
        //("outermost"), and then solve the problem as an escape.

        SatelliteMotion innermost_motion = craft_motion,
                        outermost_motion = maneuver.ResultingMotion;
        if (maneuver.ResultingMotion.Primary.IsChildOf(craft_motion.Primary))
            Utility.Swap(ref innermost_motion, ref outermost_motion);

        Debug.Assert(
            maneuver.ResultingMotion.Primary.IsChildOf(craft_motion.Primary) ||
            craft_motion.Primary.IsChildOf(maneuver.ResultingMotion.Primary),
            "Cannot enter orbit around primary's cousin in a single step.");

        int imaginary_primary_index =
            innermost_motion.Hierarchy.IndexOf(outermost_motion.Primary.Motion) - 1;

        SatelliteMotion top_level_satellite =
            innermost_motion.Hierarchy[imaginary_primary_index];

        float top_level_velocity_change =
            (outermost_motion.LocalVelocityAtDate(maneuver.Date) -
            top_level_satellite.LocalVelocityAtDate(maneuver.Date)).magnitude;

        float current_level_velocity_change = top_level_velocity_change;

        float velocity_change = top_level_velocity_change;

        bool use_energy_based_solution = true;
        if (use_energy_based_solution)
        {
            //This solution simply sums the total potential energy gained 
            //when escaping a planet/satellite and use it to calculate the 
            //necessary periapsis velocity to escape with the correct amount
            //of kinetic energy.

            //The else case solves the same problem, but considers velocity directy
            //using formula v_infinity^2 = v_initial^2 - v_escape^2
            //I implemented both in order to confirm the results of the other.

            float potential_energy_sum = 0;

            foreach (SatelliteMotion motion in innermost_motion.Hierarchy)
            {
                if (motion == top_level_satellite)
                    break;

                potential_energy_sum +=
                    -MathConstants.GravitationalConstant *
                    motion.Primary.Mass /
                    motion.AltitudeAtDate(maneuver.Date);
            }

            velocity_change = Mathf.Abs(
                Mathf.Sqrt(2 * (Mathf.Pow(top_level_velocity_change, 2) / 2 -
                                potential_energy_sum)) -
                innermost_motion.LocalVelocityAtDate(maneuver.Date).magnitude);
        }
        else
        {
            //Solve the problem for each stage in the journey
            //F.e. First you must escape the Moon, then you must still have 
            //enough speed to escape the Earth, and finally, you must still 
            //have enough speed to enter the new orbit around the sun. 

            //The problem is solved backwards. We start with the 
            //"infinity velocity" you should have upon escaping earth. This 
            //is difference between earth's velocity and the craft's velocity 
            //after the maneuver. Using the infinity velocity and the escape 
            //velocity of earth, we can calculate the dV required to reach 
            //that speed, given altitude. Then, assuming we aren't done 
            //(if the craft is directly orbiting the earth, rather than the 
            //moon, we're done), we use that dV as the new infinity velocity, 
            //in this case the velocity we should have upon escaping the moon. 
            //In both examples, the altitude used for escape velocity is 
            //derived from going one level down in the hierarchy; with earth 
            //as the primary, we use the moon's altitude. With the moon, we
            //use the craft's altitude.

            while (imaginary_primary_index > 0)
            {
                SatelliteMotion imaginary_craft_motion =
                    innermost_motion.Hierarchy[imaginary_primary_index - 1];
                Satellite imaginary_primary = imaginary_craft_motion.Primary;

                float escape_velocity = Mathf.Sqrt(
                    2 * MathConstants.GravitationalConstant *
                    imaginary_primary.Mass /
                    imaginary_craft_motion.AltitudeAtDate(maneuver.Date));

                float infinity_velocity = current_level_velocity_change;

                float periapsis_velocity = Mathf.Sqrt(Mathf.Pow(infinity_velocity, 2) +
                                                      Mathf.Pow(escape_velocity, 2));
                current_level_velocity_change = periapsis_velocity;

                velocity_change = Mathf.Abs(
                    periapsis_velocity -
                    imaginary_craft_motion.LocalVelocityAtDate(maneuver.Date).magnitude);

                imaginary_primary_index--;
            }
        }

        return velocity_change;
    }

    public float PropellentMassToUnits(float mass)
    {
        return mass / Propellent.Physical().MassPerUnit;
    }

    public float GetPropellentPurchaseCost(float propellent_mass, Market market)
    {
        return market.GetPurchaseCost(
            Propellent,
            PropellentMassToUnits(propellent_mass));
    }

    public float GetPropellentPurchaseCostPerKg(float propellent_mass, Market market)
    {
        return GetPropellentPurchaseCost(propellent_mass, market) / propellent_mass;
    }

    public float GetPropellentSaleValue(float propellent_mass, Market market)
    {
        return market.GetSaleValue(
            Propellent,
            PropellentMassToUnits(propellent_mass));
    }

    public float GetPropellentSaleValuePerKg(float propellent_mass, Market market)
    {
        return GetPropellentSaleValue(propellent_mass, market) / propellent_mass;
    }

    public float GetRefuelingCost(float target_propellent_mass, Market market)
    {
        if (target_propellent_mass > PropellentMass)
            return GetPropellentPurchaseCost(target_propellent_mass - PropellentMass, market);
        else if (target_propellent_mass < PropellentMass)
            return -GetPropellentSaleValue(PropellentMass - target_propellent_mass, market);
        else
            return 0;
    }

    public bool PurchasePropellent(float propellent_mass, Market market)
    {
        return market.Purchase(Craft.GetOwner(),
                               Craft.Cargo,
                               Propellent,
                               PropellentMassToUnits(propellent_mass));
    }

    public bool SellPropellent(float propellent_mass, Market market)
    {
        if (PropellentMass < propellent_mass)
            propellent_mass = PropellentMass;

        return market.Sell(Craft.GetOwner(), Craft.Cargo, Propellent,
               PropellentMassToUnits(propellent_mass));
    }

    public bool Refuel(float minimum_fuel_mass, Market market)
    {
        if (minimum_fuel_mass > PropellentMass)
            return PurchasePropellent(minimum_fuel_mass - 
                                      PropellentMass, market);

        return true;
    }
}