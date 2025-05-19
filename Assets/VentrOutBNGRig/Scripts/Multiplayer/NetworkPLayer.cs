using UnityEngine;
using Photon.Pun;
using UnityEngine.XR;
using TMPro;
using BNG;

namespace VentRout.Orienteering.Rig
{
    public class NetworkPlayer: MonoBehaviour
    {

        [SerializeField] private Transform headAvatar;
        [SerializeField] private Transform leftHandAvatar;
        [SerializeField] private Transform rightHandAvatar;

        [SerializeField] private GameObject meshRenderhead;
        [SerializeField] private GameObject meshRenderRight;
        [SerializeField] private GameObject meshRenderLeft;

        [SerializeField] private Animator leftHandAnimator;
        [SerializeField] private Animator rightHandAnimator;

        [SerializeField] private NetworkHeadSynchronizer HeadSynchronizer;
        [SerializeField] private NetworkHandSynchronizer HandSynchronizer;

        [SerializeField] private TMP_Text nameText;

        [SerializeField] private bool hideLocalAvatar = true;

        public float bodyHeadMaxDeltaAngle = 60;
        public float bodyRotationSpeed = 150;

        [SerializeField] private PhotonView photonView;

        private PlayerState playerState;
        private Transform rigTransform;
        private Transform headRig;
        private Transform leftHandRig;
        private Transform rightHandRig;

        void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player GameObject not found in the scene.");
                return;
            }
            rigTransform = player.transform;
            playerState = player.GetComponent<PlayerState>();
            headRig = player.transform.Find("CameraRig/TrackingSpace/CenterEyeAnchor");
            leftHandRig = player.transform.Find("CameraRig/TrackingSpace/LeftHandAnchor");
            rightHandRig = player.transform.Find("CameraRig/TrackingSpace/RightHandAnchor");

            if (photonView.IsMine)
            {
                photonView.Owner.NickName = playerState.PlayerName;
                photonView.Owner.CustomProperties["Country"] = playerState.PlayerCountry;
                photonView.Owner.CustomProperties["BibNumber"] = playerState.PlayerBibNumber;
                nameText.enabled = false;

                if (hideLocalAvatar)
                {
                    meshRenderhead.GetComponent<MeshRenderer>().enabled = false;
                    meshRenderRight.GetComponent<MeshRenderer>().enabled = false;//try removing this and dsee if it still works
                    meshRenderLeft.GetComponent<MeshRenderer>().enabled = false;

                    foreach (var item in GetComponentsInChildren<Renderer>())
                    {
                        item.enabled = false;

                    }
                }
                

            }
            else
            {
                nameText.text = photonView.Owner.NickName;

            }

        }

        void Update()
        {
            if (photonView.IsMine)
            {
                transform.position = rigTransform.position;
                var currRotation = transform.rotation.eulerAngles;
                if (Mathf.Abs(currRotation.y - headRig.rotation.eulerAngles.y) >= bodyHeadMaxDeltaAngle)
                {
                    currRotation.y = headRig.rotation.eulerAngles.y;
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(currRotation), bodyRotationSpeed * Time.deltaTime);
                }

                MapPosition(headAvatar, headRig);
                MapPosition(leftHandAvatar, leftHandRig);
                MapPosition(rightHandAvatar, rightHandRig);

                UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand), leftHandAnimator);
                UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), rightHandAnimator);
            }
            else
            {
                HeadSynchronizer.HeadLookAtPosition = headAvatar.transform.position + headAvatar.transform.forward;
                HandSynchronizer.LeftHandPosition = leftHandAvatar.position;
                HandSynchronizer.LeftHandRotation = leftHandAvatar.rotation;
                HandSynchronizer.RightHandPosition = rightHandAvatar.position;
                HandSynchronizer.RightHandRotation = rightHandAvatar.rotation;
            }

        }

        void UpdateHandAnimation(InputDevice targetDevice, Animator handAnimator)
        {
            if (targetDevice.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
            {
                handAnimator.SetFloat("Trigger", triggerValue);
            }
            else
            {
                handAnimator.SetFloat("Trigger", 0);
            }

            if (targetDevice.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
            {
                handAnimator.SetFloat("Grip", gripValue);
            }
            else
            {
                handAnimator.SetFloat("Grip", 0);
            }
        }


        void MapPosition(Transform target, Transform rigTransform)
        {
            target.position = rigTransform.position;
            target.rotation = rigTransform.rotation;
        }

    }
}