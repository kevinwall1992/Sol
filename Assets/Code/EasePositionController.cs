using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class EasePositionController : MonoBehaviour
{
    float moment = -1;

    Vector3 source_position;

    Vector3 target_position;
    public Vector3 TargetPosition
    {
        get { return target_position; }
        set
        {
            if (target_position == value)
                return;

            if (UseLocalSpace)
                source_position = transform.localPosition;
            else
                source_position = transform.position;

            target_position = value;

            moment = 0;
        }
    }

    public float Duration = 1;
    public bool UseLocalSpace = false;

    private void Update()
    {
        if (moment < 0)
            return;

        moment += Time.deltaTime / Duration;
        Vector3 position = source_position.SmoothLerped(target_position, moment);

        if (!Application.isPlaying)
            position = target_position;

        if (UseLocalSpace)
            transform.localPosition = position;
        else
            transform.position = position;
    }
}
