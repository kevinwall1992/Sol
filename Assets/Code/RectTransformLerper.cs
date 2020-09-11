using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class RectTransformLerper : MonoBehaviour
{
    public RectTransform Lerpee, A, B;
    public float LerpFactor;

    void Update()
    {
        if (Lerpee == null)
            return;

        Lerpee.localPosition = A.localPosition.Lerped(B.localPosition, LerpFactor);
        Lerpee.sizeDelta = A.sizeDelta.Lerped(B.sizeDelta, LerpFactor);
    }
}
