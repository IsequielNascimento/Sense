using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class M4Animation : MonoBehaviour
{

    [SerializeField] private Animator animator;
    // Start is called before the first frame update
    private void OnEnable()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

}