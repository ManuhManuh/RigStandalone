using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TouchTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ToggleColour();
    }

    private void ToggleColour()
    {
        GetComponent<Renderer>().material.color = Random.ColorHSV();
    }
}
