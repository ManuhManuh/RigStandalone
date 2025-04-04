using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRRigColliderAdvanced : MonoBehaviour
{
    [SerializeField] private Transform m_cam;
    [SerializeField] private CapsuleCollider m_rigCollider;

    void Update()
    {
        Vector3 newPos = m_cam.localPosition; //gets main camera position
        newPos.y = m_rigCollider.height / 2;//ignores x and z positions. Allows to move vertically
        m_rigCollider.center = newPos;//gets the collider to be set at new position
    }
}
