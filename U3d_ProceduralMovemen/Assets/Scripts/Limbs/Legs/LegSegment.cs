using UnityEngine;

namespace Limbs.Legs
{
    public class LegSegment : MonoBehaviour
    {
        [SerializeField] private Transform _start = default;
        [SerializeField] private Transform _end = default;
        public Transform Start => _start;
        public Transform End => _end;

        private float _legLength;
        public float LegLength => _legLength;

        public void Init()
        {
            _legLength = Vector3.Distance(_start.position, _end.position);
            //_start.parent = null; // transform.parent;
            //_end.parent = null; // transform.parent;
        }
    }
}