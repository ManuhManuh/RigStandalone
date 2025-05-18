using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using VentRout.Orienteering.Rig;

public class NetworkPlayerControllerObservable : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]
    private Animator _CharacterAnimator;

    private PlayerCharacterController _characterController;

    private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            _characterController = FindObjectOfType<PlayerCharacterController>();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //stream.SendNext(_characterController.CurrentVelocity);
            // remove these once we have a build that doesn't need this
            //stream.SendNext(_characterController.transform.position);
            //stream.SendNext(_characterController.transform.rotation);
        }
        else if (stream.IsReading)
        {
            var movementSpeed = (float) stream.ReceiveNext();
            _CharacterAnimator.SetFloat(MovementSpeed, movementSpeed);

        }
    }
}
