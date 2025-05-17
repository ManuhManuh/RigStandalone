using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InlandWaterFeature : MonoBehaviour
{
    public bool PlayerInWater => playerInWater;
    public float WaterLevel => waterLevel;
    [SerializeField] private float waterLevel;

    private bool playerInWater = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("XRRig"))
        {
            playerInWater = true;
            Debug.Log("Player entered water");
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("XRRig"))
        {
            playerInWater = false;
            Debug.Log("Player exited water");
        }

    }
}
