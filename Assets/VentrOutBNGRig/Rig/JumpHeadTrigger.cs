using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JumpHeadTrigger : MonoBehaviour
{
    public event Action Triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Head")
        {
            Triggered?.Invoke();
        }
    }
}
