using UnityEngine;
using System.Collections;

public interface Motion
{
    Vector3 PositionAtDate(System.DateTime date);
    Vector3 VelocityAtDate(System.DateTime date);
}