using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using System.Linq;


public class Engine : MonoBehaviour, Craft.Part
{
    public float Mass;

    public TankDictionary Tanks;

    public Mixture Propellent;
    public float ExhaustVelocity;

    public Craft Craft { get { return this.Craft(); } }

    public SatelliteMotion Motion { get { return Craft.Motion; } }

    public float PartMass { get { return Mass; } }

    public float FuelMass
    {
        get { return Tanks.Values.Sum(tank => tank.Container.ItemMass); }
    }

    void Update()
    {
        
    }

    //This method simply calculates the necessary dV and then instantaneously 
    //enters the new orbital path.
    //For maneuvers that change the craft's primary, this will alter the 
    //length of the journey from reality. However, given the extreme length
    //of interplanetary journeys compared the short time spent escaping the
    //gravity well, this should still be a very good approximation. 

    public void ApplyManeuver(Navigation.Transfer.Maneuver maneuver)
    {
        float reaction_mass = GetReactionMassRequired(maneuver);

        if (GetUsefulFuelMassAvailable() < reaction_mass)
            return;

        Propellent.Normalize();
        Dictionary<string, float> MassRequirements = 
            Propellent.Components.Keys.ToDictionary(
                component_name => component_name, 
                component_name => reaction_mass * Propellent.Components[component_name]);

        foreach (string component_name in Propellent.Components.Keys)
        {
            ItemContainer tank = Tanks[component_name].Container;

            float required_quantity = 
                MassRequirements[component_name] / 
                tank.GetItem(component_name).PhysicalItem().MassPerUnit;

            tank.TakeOut(component_name, required_quantity);
        }

        Craft.Motion = maneuver.ResultingMotion;
    }

    public float GetReactionMassRequired(Navigation.Transfer.Maneuver maneuver, 
                                         float craft_mass = -1)
    {
        if (craft_mass < 0)
            craft_mass = Craft.Mass;

        return craft_mass -
               craft_mass *
               Mathf.Pow((float)System.Math.E, -GetVelocityChangeRequired(maneuver) / 
                                               ExhaustVelocity);
    }

    public float GetVelocityChangeRequired(Navigation.Transfer.Maneuver maneuver)
    {
        //If craft's primary does not change, just take the difference in
        //velocities between current and target motion at the date of the 
        //maneuver

        if (maneuver.ResultingMotion.Primary == Craft.Primary)
            return (maneuver.ResultingMotion.LocalVelocityAtDate(maneuver.Date) - 
                    Craft.Motion.LocalVelocityAtDate(maneuver.Date))
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

        SatelliteMotion innermost_motion = Craft.Motion, 
                        outermost_motion = maneuver.ResultingMotion;
        if (maneuver.ResultingMotion.Primary.IsChildOf(Craft.Motion.Primary))
            Utility.Swap(ref innermost_motion, ref outermost_motion);

        Debug.Assert(
            maneuver.ResultingMotion.Primary.IsChildOf(Craft.Motion.Primary) || 
            Craft.Motion.Primary.IsChildOf(maneuver.ResultingMotion.Primary),
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

    public float GetUsefulFuelMassAvailable()
    {
        float min_ratio = float.MaxValue;

        foreach (string component_name in Propellent.Components.Keys)
            min_ratio = Mathf.Min(min_ratio,
                Tanks[component_name].Container.GetItem(component_name).Mass() /
                Propellent.Components[component_name]);

        float total_mass = 0;

        foreach (string component_name in Propellent.Components.Keys)
            total_mass +=
                Propellent.Components[component_name] *
                min_ratio;

        return total_mass;
    }

    public float GetMaximumUsefulFuelMass()
    {
        float min_ratio = float.MaxValue;

        foreach (string component_name in Propellent.Components.Keys)
        {
            Item component = Tanks[component_name].Container.GetItem(component_name);

            min_ratio = Mathf.Min(min_ratio,
                Tanks[component_name].Container.Volume /
                component.PhysicalItem().VolumePerUnit *
                component.PhysicalItem().MassPerUnit /
                Propellent.Components[component_name]);
        }

        float total_mass = 0;

        foreach (string component_name in Propellent.Components.Keys)
        {
            Item component = Tanks[component_name].Container.GetItem(component_name);

            total_mass +=
                Propellent.Components[component_name] *
                min_ratio;
        }

        return total_mass;
    }

    public float GetVelocityChangeAvailable()
    {
        return ExhaustVelocity *
                Mathf.Log(Craft.Mass / (Craft.Mass - GetUsefulFuelMassAvailable()));
    }

    public float GetMaximumVelocityChange()
    {
        float maximum_useful_fuel_mass = GetMaximumUsefulFuelMass();
        float maximum_craft_mass = (Craft.Mass - FuelMass) + maximum_useful_fuel_mass;

        return ExhaustVelocity *
                Mathf.Log(maximum_craft_mass /
                            (maximum_craft_mass - maximum_useful_fuel_mass));
    }


    [System.Serializable]
    public class TankDictionary : SerializableDictionaryBase<string, FluidContainer> { }
}

[System.Serializable]
public class Mixture
{
    public ComponentDictionary Components;

    public void Normalize()
    {
        float total_weight = Components.Values.Sum();

        foreach (string component_name in Components.Keys.ToList())
            Components[component_name] /= total_weight;
    }
    
    [System.Serializable]
    public class ComponentDictionary : SerializableDictionaryBase<string, float> { }
}