using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public GameObject rig;

    public Animator animator;
    Vector2 speedPercent;

    void Update()
    {
        if (GetComponent<Movement>().grounded) speedPercent = new Vector2(Mathf.Clamp(transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).x, -1f, 1f), Mathf.Clamp( transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).z, -1f, 1f ) );
        else speedPercent = new Vector2(10f, 10f);
        animator.SetFloat("Xaxis", speedPercent.x, 0.1f, Time.deltaTime);
        animator.SetFloat("Yaxis", speedPercent.y, 0.1f, Time.deltaTime);
    }
}
