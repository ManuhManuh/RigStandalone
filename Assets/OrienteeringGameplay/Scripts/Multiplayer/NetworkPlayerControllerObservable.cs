using Photon.Pun;
using UnityEngine;
using VentRout.Orienteering.Rig;

namespace VentRout.Orienteering.Gameplay
{
    public class NetworkPlayerControllerObservable : MonoBehaviourPunCallbacks, IPunObservable
    {

        [SerializeField] private Animator _CharacterAnimator;
        private PlayerCharacterController characterController;
        private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");

        void Start()
        {
            if (photonView.IsMine)
            {
                characterController = FindFirstObjectByType<PlayerCharacterController>();
                //_characterController = FindObjectOfType<PlayerCharacterController>();
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (_CharacterAnimator == null) return;

                //stream.SendNext(_characterController.CurrentVelocity);

            }
            else if (stream.IsReading)
            {
                if (_CharacterAnimator == null) return;

                var movementSpeed = (float)stream.ReceiveNext();
                _CharacterAnimator.SetFloat(MovementSpeed, movementSpeed);

            }
        }
    }
}