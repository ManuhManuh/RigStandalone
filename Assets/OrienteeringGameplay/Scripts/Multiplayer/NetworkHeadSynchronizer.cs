using UnityEngine;

namespace VentRout.Orienteering.Gameplay
{
    [RequireComponent(typeof(Animator))]
    public class NetworkHeadSynchronizer : MonoBehaviour
    {
        private Animator _animator;

        public Vector3 HeadLookAtPosition { get; set; }

        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            _animator.SetLookAtWeight(1f);
            _animator.SetLookAtPosition(HeadLookAtPosition);
        }
    }
}