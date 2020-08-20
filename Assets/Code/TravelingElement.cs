using UnityEngine;
using System.Collections;

public class TravelingElement : MonoBehaviour
{
    public Transform OriginalParent;

    public TravelingElementHotel Destination;

    public bool DontModifyLocalTransform = true;

    void Start()
    {
        OriginalParent = transform.parent;
    }

    void Update()
    {
        if(transform.parent == OriginalParent)
            transform.SetParent(Destination.transform,
                                !DontModifyLocalTransform);

    }
}
