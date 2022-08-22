using UnityEngine;

public class Movement : MonoBehaviour, IMovement
{
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _moveSmoothTime = 1f;
    [SerializeField] private float _hoverHeight = 1f;
    [SerializeField] private float _fallSpeed = 9.81f;
    [SerializeField] private float _fallSmoothTime = 0.1f;
    [SerializeField] private float _rotSmoothTime = 0.1f;

    [SerializeField] private Rigidbody _rigidbody = default;

    private Vector3 _direction = Vector3.zero;
    private Vector3 _moveDamp = Vector3.zero;
    private Vector2 _input2;
    private Vector3 _inputDirection;

    private (Vector3, Vector3) _camDirections;
    private Vector3 _input3;

    private Vector3 _hoverPos;
    private Vector3 _hoverDamp;
    private Vector3 _normal;

    private Vector3 _rotDamp;
    public Transform Camera { get; set; }
    public PlayerInputSystem PlayerInput { get; set; }


    private void Update()
    {
        _input2 = PlayerInput.PlayerControl.Player.Move.ReadValue<Vector2>();
        _input3 = new Vector3(_input2.x, 0, _input2.y);
        _camDirections.Item1 = Camera.right;
        _camDirections.Item2 = Camera.forward;
        _camDirections.Item1.y = 0;
        _camDirections.Item2.y = 0;
        Vector3.Normalize(_camDirections.Item1);
        Vector3.Normalize(_camDirections.Item2);

        _inputDirection = _camDirections.Item1 * _input3.x + _camDirections.Item2 * _input3.z;

        _rigidbody.rotation = SmoothDampRotation(_rigidbody.rotation,RotationBySurface(_rigidbody.rotation));

        //_input = _playerControl.Player.Move.ReadValue<Vector2>();
        //_controlDir = (_cameraPivot.forward * _input.y + _cameraPivot.right * _input.x).normalized;
        //
        //// transform movement
        //var targetMoveAmount = Vector3.zero;
        //
        //    if (IsInControl)
        //    targetMoveAmount = _controlDir * _currentSpeed;
        //
        ////_moveDirection = Vector3.SmoothDamp(_moveDirection, targetMoveAmount, ref _lerpMove, _lerpMoveTime);
        //_moveDirection = targetMoveAmount;
        //
        //// root rotation
        //_angle = Vector3.SignedAngle(-_root.forward, _controlDir, _root.up);
        //_angle = SharedCalculations.ClampAngle(_angle - 90);
        //
        //if (_input.magnitude > 0.01f)
        //{
        //    _addRotation = Quaternion.Euler(Vector3.up * _angle);
        //    _rootRotation = _root.localRotation;
        //    _rootRotation = SharedCalculations.SmoothDampQuaternion(
        //        _rootRotation, _rootRotation * _addRotation, ref _rotateLerp, _lerpRotateTime);
        //
        //    _root.localRotation = _rootRotation;
        //}
    }

    private void FixedUpdate()
    {
        var deltaTime = Time.fixedDeltaTime;
        _direction = SmoothDampMove(deltaTime);
        _direction = DirectionBySurface(_direction);
        var move = SmoothDampHover(deltaTime) + _direction;
        _rigidbody.MovePosition(move);
    }

    private Vector3 SmoothDampMove(float deltaTime)
    {
        return Vector3.SmoothDamp(_direction, _inputDirection,
            ref _moveDamp, _moveSmoothTime, _moveSpeed, deltaTime);
    }

    private Vector3 SmoothDampHover(float deltaTime)
    {
        _hoverPos = _rigidbody.position + Vector3.down;

        if (Physics.Raycast(_rigidbody.position, Vector3.down, out var hit))
            _hoverPos = hit.point + Vector3.up * _hoverHeight;

        return Vector3.SmoothDamp(_rigidbody.position, _hoverPos,
            ref _hoverDamp, _fallSmoothTime, _fallSpeed, deltaTime);
    }

    private Vector3 DirectionBySurface(Vector3 direction)
    {
        _normal = Vector3.zero;
        if (Physics.Raycast(_rigidbody.position, -transform.up, out var _hit))
            _normal = _hit.normal;

        return direction - Vector3.Dot(direction, _normal) * _normal;
    }

    private Quaternion RotationBySurface(Quaternion rotation)
    {
        if (Physics.Raycast(_rigidbody.position, Vector3.down, out var hit))
            rotation = Quaternion.FromToRotation(rotation * Vector3.up, hit.normal) * rotation;

        return rotation;
    }

    private Quaternion SmoothDampRotation(Quaternion from, Quaternion to)
    {
        var currentEuler = from.eulerAngles;
        var targetEuler = to.eulerAngles;
        
        return Quaternion.Euler(
            Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref _rotDamp.x, _rotSmoothTime),
            Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref _rotDamp.y, _rotSmoothTime),
            Mathf.SmoothDampAngle(currentEuler.z, targetEuler.z, ref _rotDamp.z, _rotSmoothTime)
        );
    }
}