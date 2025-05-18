using UnityEngine;
using Photon.Pun;
using UnityEngine.XR;
using TMPro;
using BNG;

public class NetworkPLayer : MonoBehaviour
{
    private PhotonView photonView;

    public Transform head;//do i need this. duplicate
    public Transform leftHand;//do i need this. duplicate
    public Transform rightHand;//do i need this. duplicate
    
    public Animator leftHandAnimator;
    public Animator rightHandAnimator;

    public Transform headRig;
    public Transform leftHandRig;
    public Transform rightHandRig;

    public GameObject meshRenderhead;
    public GameObject meshRenderRight; //right hand
    public GameObject meshRenderLeft; //left hand

    public NetworkHeadSynchronizer HeadSynchronizer;
    public NetworkHandSynchronizer HandSynchronizer;

    public TMP_Text nameText;

    public float bodyHeadMaxDeltaAngle = 60;
    public float bodyRotationSpeed = 150;

    private Transform _rigTransform;
    public GameObject headlamp;

    void Start()
    {
        photonView = GetComponent<PhotonView>();
        XRRigOrigin rig = FindObjectOfType<XRRigOrigin>();
        if (rig != null)
        {
            _rigTransform = rig.transform;
            headRig = rig.transform.Find("Camera Offset/Main Camera");
            leftHandRig = rig.transform.Find("Camera Offset/Left Hand");
            rightHandRig = rig.transform.Find("Camera Offset/Right Hand");
        }

        else
        {
            BNGPlayerController player = FindObjectOfType<BNGPlayerController>();
            _rigTransform = player.transform;
            headRig = player.transform.Find("CameraRig/TrackingSpace/CenterEyeAnchor");
            leftHandRig = player.transform.Find("CameraRig/TrackingSpace/LeftHandAnchor/LeftControllerAnchor");
            rightHandRig = player.transform.Find("CameraRig/TrackingSpace/RightHandAnchor/RightControllerAnchor");
        }


        if (photonView.IsMine)
        {
            //photonView.Owner.NickName = PlayerPrefs.GetString("Nickname");
            //photonView.Owner.NickName = PlayerState.Instance.PlayerName;
            //photonView.Owner.CustomProperties["Country"] = PlayerState.Instance.PlayerCountry;
            //photonView.Owner.CustomProperties["BibNumber"] = PlayerState.Instance.PlayerBibNumber;
            //nameText.enabled = false;
   
            meshRenderhead.GetComponent<MeshRenderer>().enabled = false;
            meshRenderRight.GetComponent<MeshRenderer>().enabled = false;//try removing this and dsee if it still works
            meshRenderLeft.GetComponent<MeshRenderer>().enabled = false;

            foreach (var item in GetComponentsInChildren<Renderer>())
            {
                item.enabled = false;

            }

            //RowManager rowManager = FindObjectOfType<RowManager>();
            //if (rowManager != null)
            //{
            //    rowManager._currentPlayerNickName = photonView.Owner.NickName;//other option is playerprefs.getstring...
            //    rowManager._currentPlayerUserId = photonView.Owner.UserId;
            //}
        }

        else
        {
            nameText.text = photonView.Owner.NickName;

        }

    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            transform.position = _rigTransform.position;
            var currRotation = transform.rotation.eulerAngles;
            if (Mathf.Abs(currRotation.y - headRig.rotation.eulerAngles.y) >= bodyHeadMaxDeltaAngle)
            {
                currRotation.y = headRig.rotation.eulerAngles.y;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(currRotation), bodyRotationSpeed * Time.deltaTime);
            }

            MapPosition(head, headRig);
            MapPosition(leftHand, leftHandRig);
            MapPosition(rightHand, rightHandRig);

            UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.LeftHand), leftHandAnimator);
            UpdateHandAnimation(InputDevices.GetDeviceAtXRNode(XRNode.RightHand), rightHandAnimator);
        }
        else
        {
            HeadSynchronizer.HeadLookAtPosition = head.transform.position + head.transform.forward;
            HandSynchronizer.LeftHandPosition = leftHand.position;
            HandSynchronizer.LeftHandRotation = leftHand.rotation;
            HandSynchronizer.RightHandPosition = rightHand.position;
            HandSynchronizer.RightHandRotation = rightHand.rotation;
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
