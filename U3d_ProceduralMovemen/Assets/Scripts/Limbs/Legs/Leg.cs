using UnityEngine;

namespace Limbs.Legs
{
    public class Leg : MonoBehaviour
    {
        private const int IkIterations = 1;
        private const float MinimalLength = 0.000001f;

        /// <summary>
        /// Offset to start step on percent of the 100% cycle
        /// </summary>
        [SerializeField, Range(0, 100f), Tooltip("Percents")]
        private float _startStepOffset = 0;

        [SerializeField, Range(0.01f, 99.99f), Tooltip("Percents")]
        private float _stepDuration = 50;

        [SerializeField] private float _stepHeight = 0.2f;
        [SerializeField] private LegSegment[] _legSegments = default;

        /// <summary>
        /// count from 0 to 1 for lerp method time
        /// </summary>
        public float CountTime { get; set; }
        public Vector3 TargetPosition { get; set; }
        public Vector3 NextPos { get; set; }
        public Vector3 LastPos { get; set; }
        public float CountDuration { get; set; }
        public bool IsMoving { get; set; }

        public Vector3 CurPos
        {
            get => _curPos;
            set
            {
                _curPos = value;

                if (_segments != null && _segments.Length > 0)
                    _segments[_segments.Length - 1].Pos = value;
            }
        }

        public float StartStepOffset => _startStepOffset;
        public float StepDuration => _stepDuration;
        public float StepHeight => _stepHeight;

        private Vector3 _curPos;
        private Vector3 _localJoint;

        private Segment[] _segments;
        private Transform _parent;

        private Vector3 _prevSegEnd;
        private Vector3 _nextSegEnd;
        private Vector3 _startToEnd;
        private Vector3 _upDir;
        private Vector3 _startProject;
        private Vector3 _upProject;
        private float _angle;

        private void Start()
        {
            _parent = transform.parent;
            _localJoint = transform.localPosition;

            foreach (var segment in _legSegments)
                segment.Init();

            // SEGMENTS
            // add two more segment (copy first segment as joint and current (Target) point as last segment)
            _segments = new Segment[_legSegments.Length + 2];

            for (var i = 0; i < _legSegments.Length; i++)
            {
                _segments[i + 1] = new Segment
                {
                    Pos = _legSegments[i].Start.position,
                    Dir = _legSegments[i].Start.forward,
                    Len = Vector3.Distance(_legSegments[i].Start.position, _legSegments[i].End.position)
                };
            }

            _segments[0] = new Segment
            {
                Pos = _parent.TransformPoint(_localJoint), Dir = _segments[1].Dir, Len = MinimalLength
            };

            _segments[_segments.Length - 1] = new Segment
            {
                Pos = _curPos, Dir = Vector3.zero, Len = MinimalLength
            };
        }

        private void Update()
        {
            _segments[0].Pos = _parent.TransformPoint(_localJoint);
            _segments[0].Dir = _segments[1].Dir;

            for (var i = 0; i < IkIterations; i++)
            {
                CalculatePositionsBackward();
                CalculatePositionsForward();
                CalculateRotateSegments();
            }

            ApplyToTransforms();
        }

        private void CalculatePositionsBackward()
        {
            for (var i = _segments.Length - 2; i > 0; i--)
                PositionBackward(_segments[i + 1], _segments[i], _segments[i - 1]);
        }

        private void CalculatePositionsForward()
        {
            for (var i = 1; i < _segments.Length - 1; i++)
                PositionForward(_segments[i - 1], _segments[i], _segments[i + 1]);
        }

        private void CalculateRotateSegments()
        {
            for (var i = 1; i < _segments.Length - 2; i++)
                RotateSegment(_segments[i], _segments[i + 1], _segments[i + 2]);
        }

        private void PositionBackward(Segment prevSegment, Segment curSegment, Segment nextSegment)
        {
            _nextSegEnd = nextSegment.Pos + nextSegment.Dir * nextSegment.Len;
            curSegment.Dir = (prevSegment.Pos - _nextSegEnd).normalized;
            curSegment.Pos = prevSegment.Pos - curSegment.Dir * curSegment.Len;
        }

        private void PositionForward(Segment prevSegment, Segment curSegment, Segment nextSegment)
        {
            _prevSegEnd = prevSegment.Pos + prevSegment.Dir * prevSegment.Len;
            curSegment.Pos = _prevSegEnd;
            curSegment.Dir = (nextSegment.Pos - curSegment.Pos).normalized;
        }

        private void RotateSegment(Segment startSegment, Segment middleSegment, Segment endSegment)
        {
            // rotation part to point in correct direction
            _startToEnd = endSegment.Pos - startSegment.Pos;
            _upDir = transform.parent.up; // Vector3.up;  

            _startProject = Vector3.ProjectOnPlane(startSegment.Dir, _startToEnd);
            _upProject = Vector3.ProjectOnPlane(_upDir, _startToEnd);
            _angle = Vector3.SignedAngle(_startProject, _upProject, _startToEnd);

            startSegment.Dir = Quaternion.AngleAxis(_angle, _startToEnd) * startSegment.Dir.normalized;
            middleSegment.Pos = startSegment.Pos + startSegment.Dir * startSegment.Len;
            middleSegment.Dir = endSegment.Pos - middleSegment.Pos;
        }

        private void ApplyToTransforms()
        {
            for (var i = 0; i < _legSegments.Length; i++)
            {
                _legSegments[i].transform.position = _segments[i + 1].Pos;
                _legSegments[i].transform.rotation = Quaternion.LookRotation(_segments[i + 1].Dir);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            if (_segments != null)
                for (int i = 1; i < _segments.Length - 1; i++)
                {
                    Gizmos.DrawWireSphere(_segments[i].Pos, 0.1f);
                    Gizmos.DrawLine(_segments[i].Pos, _segments[i + 1].Pos);
                }
        }
        
        private class Segment
        {
            public Vector3 Pos;
            public Vector3 Dir;
            public float Len;
        }
    }
}