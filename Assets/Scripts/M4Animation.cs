using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class M4Animation : MonoBehaviour
{

    [SerializeField] private Animator animator;
    private void OnEnable()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

}
