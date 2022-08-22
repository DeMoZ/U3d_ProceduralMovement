using System;
using UnityEngine;

public class HoverBody : MonoBehaviour
{
    [SerializeField] private float _hoverHeight = 1f;
    [SerializeField] private float _fallSpeed = 0.2f;

    private Rigidbody _rigidbody;
    private Vector3 _hoverDamp;
    private Vector3 _hoverPos;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        _hoverPos = _rigidbody.position + Vector3.down;

        if (Physics.Raycast(_rigidbody.position, Vector3.down, out var hit))
            _hoverPos = hit.point + Vector3.up * _hoverHeight;

        var move = Vector3.SmoothDamp(_rigidbody.position, _hoverPos,
            ref _hoverDamp, _fallSpeed);

        _rigidbody.MovePosition(move);
    }
}