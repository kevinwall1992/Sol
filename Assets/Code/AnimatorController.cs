using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class AnimatorController : MonoBehaviour
{
    public float Moment = 0;

    public Animator Animator;

    private void Update()
    {
        Moment = Mathf.Clamp(Moment, 0, 1);

        if(Animator.gameObject.activeSelf)
            Animator.SetFloat("Moment", Moment);
    }
}
