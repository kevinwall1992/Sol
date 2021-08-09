using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Room))]
public class Housing : MonoBehaviour
{
    public float CeilingHeight = 2.4f;

    public float SquareFootage
    {
        get
        {
            return GetComponents<Pocket>()
                .Where(pocket => pocket.Type == PocketType.Living)
                .Sum(pocket => pocket.Size) / 
                CeilingHeight;
        }
    }
}
