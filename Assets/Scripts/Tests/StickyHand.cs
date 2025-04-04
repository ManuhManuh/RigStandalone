using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class StickyHand : MonoBehaviour
{
    // This script is to be applied to a grabbable object (BNG) where
    // the object will stick to one hand until the other hand grabs it.

    // Grabbable object should have the Grab Type set to "Inherit" in the inspector

    [SerializeField] private bool stickToLeftHand = true;
    [SerializeField] private Grabbable grabbable;

    // invoke this function when the object is grabbed
    public void SetDroppable()
    {
        // used in the race scene
        if (stickToLeftHand)
        {
            // can only be dropped if it is in the right hand
            grabbable.CanBeDropped = grabbable.GetPrimaryGrabber().HandSide == ControllerHand.Right;
        }
        else
        {
            // can only be dropped if it is in the left
            // hand
            grabbable.CanBeDropped = grabbable.GetPrimaryGrabber().HandSide == ControllerHand.Left;
        }
    }

}
