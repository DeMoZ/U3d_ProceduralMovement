using System;
using UnityEngine;

public class PlayerInputSystem
{
    public PlayerControl PlayerControl { get; private set; }

    public  PlayerInputSystem(PlayerControl playerControl)
    {
        PlayerControl = playerControl;
        PlayerControl.Player.TransformAir.performed +=_=> Jump();
    }

    public void Enable() => PlayerControl.Enable();

    public void Disable() => PlayerControl.Disable();
    
    private void Jump() => Debug.LogWarning("Jump pressed");
}