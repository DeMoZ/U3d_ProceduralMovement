using UnityEngine;

public interface IMovement
{
    Transform Camera { get; set; }
    PlayerInputSystem PlayerInput { get; set; }
}