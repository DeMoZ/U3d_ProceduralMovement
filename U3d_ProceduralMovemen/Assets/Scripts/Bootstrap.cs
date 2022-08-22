using System.Collections.Generic;
using UnityEngine;
using System;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private Camera _camera = default;
    [SerializeField] private List<Actor> _actors= default;

    private void Start()
    {
        var playerControl = new PlayerControl();
        var playerInput = new PlayerInputSystem(playerControl);
        playerInput.Enable();

        foreach (var actor in _actors)
        {
            if (!actor.spawn) continue;
            
            var body = Instantiate(actor.Body, actor.StartPosition.position, Quaternion.identity);
            var movement = body.GetComponent<IMovement>();
            movement.Camera = _camera.transform;
            movement.PlayerInput = playerInput;    
        }
    }
}

[Serializable]
public class Actor
{
    public GameObject Body;
    public Transform StartPosition;
    public bool spawn;
}