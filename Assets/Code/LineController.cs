using UnityEngine;
using System.Collections;
using System.Linq;

[ExecuteAlways]
public class LineController : MonoBehaviour
{
    public LineRenderer Line;

    public System.Func<float, Vector3> SamplingFunction;
    public int SampleCount = 100;

    public float Length
    {
        get
        {
            float sum = 0;

            for (int i = 0; i < Line.positionCount - 1; i++)
            {
                sum += Line.GetPosition(i).Distance(Line.GetPosition(i + 1));
            }

            if(Line.loop)
                sum += Line.GetPosition(Line.positionCount - 1).Distance(Line.GetPosition(0));

            return sum;
        }
    }

    void Update()
    {
        if (Line == null || Line.enabled == false || SamplingFunction == null)
            return;

        Line.positionCount = SampleCount;
        for (int i = 0; i < SampleCount; i++)
            Line.SetPosition(i, SamplingFunction(i / (float)(SampleCount - 1)));
    }
}
