using UnityEngine;


namespace PhotonTutorial.Utilities
{
    [ExecuteAlways]
    public class BillBoard : MonoBehaviour
    {

        private Transform mainCameraTransform;

        // Start is called before the first frame update
        void Start()
        {
            mainCameraTransform = Camera.main.transform;
        }

        // Update is called once per frame
        void LateUpdate()
        {
            transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
                mainCameraTransform.rotation * Vector3.up);
        }
    }
}