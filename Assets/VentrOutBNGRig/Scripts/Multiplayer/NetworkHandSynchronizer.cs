using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NetworkHandSynchronizer : MonoBehaviour
{
    private Animator _animator;

    public Vector3 LeftHandRotationOffset;
    public Vector3 RightHandRotationOffset;
    
    public Vector3 LeftHandPosition { get; set; }
    public Quaternion LeftHandRotation { get; set; }
    public Vector3 RightHandPosition { get; set; }
    public Quaternion RightHandRotation { get; set; }
    
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {

        _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        _animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandPosition);

        var lRot = LeftHandRotation * Quaternion.Euler(LeftHandRotationOffset);
        _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
        _animator.SetIKRotation(AvatarIKGoal.LeftHand, lRot);
        
        _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        _animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandPosition);
        var rRot = RightHandRotation * Quaternion.Euler(RightHandRotationOffset);
        _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        _animator.SetIKRotation(AvatarIKGoal.RightHand, rRot);
    }
}
