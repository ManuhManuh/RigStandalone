using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRRigOrigin : MonoBehaviour
{
    [SerializeField] private Transform head;//be able to get value but  not set value
    [SerializeField] private Transform rHand;
    [SerializeField] private Transform lHand;

    public Transform getHead()
    {
        return head;
    }

    public Transform getrHand()
    {
        return rHand;
    }

    public Transform getlHand()
    {
        return lHand;
    }
}
