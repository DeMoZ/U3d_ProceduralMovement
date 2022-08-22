using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Limbs.Legs
{
    public class StepController : MonoBehaviour
    {
        [SerializeField] private float _cycleTime = 1f;
        [SerializeField] private float _rayLength = 2;
        [SerializeField] private Leg[] _legs = default;
        [SerializeField] private float _gizmoSize = 0.2f;

        private int _legsAmount;
        private RaycastHit _hit;
        private Vector3 _raycastPoint;

        private Vector3 _lastPosition;

        private float _cycleTimer;
        private float _cycleTimerPercents;
        private float _deltaTime;
        private float _deltaTimePercent;

        private Vector3 _velocity;
        private float _velocityMod;

        private Coroutine _timerRoutine;

        private void Start()
        {
            _legsAmount = _legs.Length;

            if (_legsAmount <= 0) return;

            _velocity = Vector3.zero;
            _lastPosition = transform.position;

            for (int i = 0; i < _legsAmount; i++)
            {
                TargetPosition(_legs[i]);

                _legs[i].NextPos = _legs[i].TargetPosition;
                _legs[i].CurPos = _legs[i].TargetPosition;
            }
        }

        private void Update()
        {
            _velocity = transform.position - _lastPosition;

            if (_timerRoutine == null)
                for (int i = 0; i < _legsAmount; i++)
                    TargetPosition(_legs[i]);


            StartTimerRoutine();
            StopTimerRoutine();

            if (_timerRoutine == null)
                _lastPosition = transform.position;
        }

        private void StopTimerRoutine()
        {
            if (_timerRoutine != null &&
                ZeroVelocity() &&
                !ALegIsMoving() &&
                !ALegHasDifferentTargetCurrent())
            {
                StopCoroutine(_timerRoutine);
                _timerRoutine = null;
                // Debug.Log($"Coroutine stoped");
            }
        }

        private void StartTimerRoutine()
        {
            if (_timerRoutine == null &&
                !ZeroVelocity() &&
                !ALegIsMoving() &&
                ALegHasDifferentTargetCurrent())
            {
                //Debug.Log($"Coroutine started");
                _timerRoutine = StartCoroutine(nameof(TimerRoutine));
            }
        }

        private IEnumerator TimerRoutine()
        {
            _cycleTimer = 0;

            while (true)
            {
                yield return null;

                _velocityMod = _velocity.magnitude;
                _velocityMod = (_velocityMod > 0) ? Mathf.Clamp(_velocityMod, 0.02f, float.MaxValue) : 0;

                _deltaTime = Time.deltaTime + _velocityMod;
                _deltaTimePercent = (_deltaTime / _cycleTime) * 100;
                _cycleTimer = _cycleTimer > _cycleTime ? 0 : _cycleTimer + _deltaTime;

                for (int i = 0; i < _legsAmount; i++)
                {
                    TargetPosition(_legs[i]);
                    NextOnCircleTimer(_legs[i]);
                    LerpSinMoving(_legs[i], _deltaTimePercent);
                }

                _lastPosition = transform.position;
            }
        }

        private bool ZeroVelocity() =>
            Vector3Equal(_velocity, Vector3.zero);

        private bool ALegHasDifferentTargetCurrent() =>
            _legs.Sum(l => Vector3Equal(l.CurPos, l.TargetPosition) ? 0 : 1) > 0;

        private bool ALegIsMoving() =>
            _legs.Sum(l => l.IsMoving ? 1 : 0) > 0;

        private void NextOnCircleTimer(Leg leg)
        {
            _cycleTimerPercents = _cycleTimer / _cycleTime * 100;
            if (!leg.IsMoving &&
                _cycleTimerPercents >= leg.StartStepOffset &&
                _cycleTimerPercents < leg.StartStepOffset + leg.StepDuration &&
                !Vector3Equal(leg.CurPos, leg.TargetPosition))
            {
                leg.IsMoving = true;
                leg.LastPos = leg.CurPos;
                leg.CountDuration = 0;
                // Debug.LogWarning($"Start {leg.name}");
            }
        }

        private bool Vector3Equal(Vector3 a, Vector3 b) =>
            Vector3.SqrMagnitude(a - b) < 0.0001f;

        private void LerpSinMoving(Leg leg, float deltaPercents)
        {
            if (leg.IsMoving)
            {
                leg.CountDuration += deltaPercents;
                leg.CountTime = leg.CountDuration / leg.StepDuration;

                leg.NextPos = leg.TargetPosition;
                leg.CurPos = Vector3.Lerp(leg.LastPos, leg.NextPos, leg.CountTime);
                leg.CurPos += transform.up * Mathf.Sin(leg.CountTime * Mathf.PI) * leg.StepHeight;

                if (leg.CountTime >= 1)
                {
                    leg.IsMoving = false;
                    leg.CurPos = leg.TargetPosition;
                    // Debug.LogWarning($"stoP {leg.name}");
                }
            }
        }

        private void TargetPosition(Leg leg)
        {
            _raycastPoint = leg.transform.position + leg.transform.forward /* +
                            _velocity.normalized * _stepVelocityLength*/;

            if (Physics.Raycast(_raycastPoint, -transform.up, out _hit, _rayLength))
                leg.TargetPosition = _hit.point;
            else if (Physics.Raycast(_raycastPoint, transform.up, out _hit, _rayLength))
                leg.TargetPosition = _hit.point;
            else
                leg.TargetPosition = _raycastPoint;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < _legsAmount; i++)
                Gizmos.DrawWireSphere(_legs[i].TargetPosition, _gizmoSize);

            Gizmos.color = Color.yellow;
            for (int i = 0; i < _legsAmount; i++)
                Gizmos.DrawWireSphere(_legs[i].NextPos, _gizmoSize);

            Gizmos.color = Color.green;
            for (int i = 0; i < _legsAmount; i++)
                Gizmos.DrawSphere(_legs[i].CurPos, _gizmoSize);
        }
    }
}