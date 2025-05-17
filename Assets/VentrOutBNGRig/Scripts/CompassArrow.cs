using Unity.Mathematics;
using UnityEngine;
using VentRout.Orienteering.Gameplay;

namespace VentRout.Orienteering.Rig
{
    public class CompassArrow : MonoBehaviour
    {
        public Transform NorthOverride;

        [SerializeField] private float maxAngularVelocity;

        [Tooltip("The axis, in local coordinates, around which the arrow spins")] [SerializeField]
        private Vector3 localRotationAxis;

        private void FixedUpdate()
        {
            Transform northPole = TrueNorth.Instance.transform;
            if (NorthOverride)
            {
                northPole = NorthOverride;
            }

            Vector3 northDirection = Vector3.forward;
            if (northPole)
            {
                northDirection = northPole.position - transform.position;
            }

            Vector3 rotationAxis = transform.parent.TransformVector(localRotationAxis);
            Vector3 toNorthProjected = Vector3.ProjectOnPlane(northDirection,
                rotationAxis);

            Quaternion goalOrientation = Quaternion.LookRotation(toNorthProjected, rotationAxis);
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goalOrientation,
                maxAngularVelocity * Time.fixedDeltaTime);
        }
    }
}