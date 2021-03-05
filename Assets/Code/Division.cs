using UnityEngine;
using System.Collections;

//Divisions are profit making pieces of a Company.
//In the future, they will be required to provide financial data
//that will be used to determine funding.

public class Division : User.Script
{
    public Meeting Meeting;

    protected virtual void Start()
    {
        Meeting.AddTask(ConductBusiness);
    }

    protected virtual void ConductBusiness() { }
}
